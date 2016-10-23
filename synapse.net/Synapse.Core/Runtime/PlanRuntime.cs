using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core.DataAccessLayer;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public partial class Plan
    {
        #region events
        public event EventHandler<HandlerProgressCancelEventArgs> Progress;
        public event EventHandler<LogMessageEventArgs> LogMessage;

        /// <summary>
        /// Notify of step start. If return value is True, then cancel operation.
        /// </summary>
        /// <param name="context">The method name or workflow activty.</param>
        /// <param name="message">Descriptive message.</param>
        /// <param name="status">Overall Package status indicator.</param>
        /// <param name="id">Message Id.</param>
        /// <param name="sequence">Message/error severity.</param>
        /// <param name="ex">Current exception (optional).</param>
        /// <returns>HandlerProgressCancelEventArgs.Cancel value.</returns>
        protected virtual bool OnProgress(string actionName, string context, string message, StatusType status = StatusType.Running, long id = 0, int sequence = 0, bool cancel = false, Exception ex = null)
        {
            HandlerProgressCancelEventArgs e =
                new HandlerProgressCancelEventArgs( context, message, status, id, sequence, cancel, ex ) { ActionName = actionName };
            if( _wantsCancel ) { e.Cancel = true; }
            OnProgress( e );

            return e.Cancel;
        }

        /// <summary>
        /// Notify of step start. If e.Cancel is True, then cancel operation.
        /// </summary>
        protected virtual void OnProgress(HandlerProgressCancelEventArgs e)
        {
            Progress?.Invoke( this, e );
        }

        protected virtual void OnLogMessage(LogMessageEventArgs e)
        {
            LogMessage?.Invoke( this, e );
        }
        #endregion

        bool _wantsCancel = false;
        bool _wantsPause = false;

        Dictionary<string, ParameterInfo> _configSets = new Dictionary<string, ParameterInfo>();
        Dictionary<string, ParameterInfo> _paramSets = new Dictionary<string, ParameterInfo>();

        #region control methods
        public void Stop() { _wantsCancel = true; }
        public void Pause() { _wantsPause = true; }
        public void Continue() { _wantsPause = false; }
        public bool IsPaused { get { return _wantsPause; } }
        void CheckPause() //todo: this is just a stub method
        {
            if( _wantsPause )
            {
                System.Threading.Thread.Sleep( 1000 );
            }
        }

        bool WantsStopOrPause()
        {
            if( _wantsCancel )
            {
                return true;
            }
            else
            {
                if( _wantsPause )
                {
                    System.Threading.Thread.Sleep( 1000 );
                }
                return false;
            }
        }
        #endregion

        public ExecuteResult Start(Dictionary<string, string> dynamicData, bool dryRun = false, bool inProc = true)
        {
            SynapseDal.CreateDatabase();
            CreateInstance();

            if( inProc )
                Result = ProcessRecursive( RunAs, null, Actions, ExecuteResult.Emtpy, dynamicData, dryRun, ExecuteHandlerProcessInProc );
            else
                Result = ProcessRecursive( RunAs, null, Actions, ExecuteResult.Emtpy, dynamicData, dryRun, ExecuteHandlerProcessExternal );

            UpdateInstanceStatus( Result.Status, Result.Status.ToString() );

            return Result;
        }

        ExecuteResult ProcessRecursive(SecurityContext parentSecurityContext, ActionItem actionGroup, List<ActionItem> actions, ExecuteResult result,
            Dictionary<string, string> dynamicData, bool dryRun,
            Func<SecurityContext, ActionItem, Dictionary<string, string>, bool, ExecuteResult> executeHandlerMethod)
        {
            if( WantsStopOrPause() ) { return result; }

            ExecuteResult returnResult = ExecuteResult.Emtpy;

            StatusType queryStatus = result.Status;
            if( actionGroup != null && (actionGroup.ExecuteCase == result.Status || actionGroup.ExecuteCase == StatusType.Any) )
            {
                ExecuteResult r = executeHandlerMethod( parentSecurityContext, actionGroup, dynamicData, dryRun );
                if( r.Status > returnResult.Status )
                    returnResult = r;

                if( actionGroup.HasActions )
                {
                    r = ProcessRecursive( parentSecurityContext, null, actionGroup.Actions, r, dynamicData, dryRun, executeHandlerMethod );
                    if( r.Status > returnResult.Status )
                        returnResult = r;
                }

                if( r.Status > queryStatus )
                    queryStatus = r.Status;
            }

            IEnumerable<ActionItem> actionList =
                actions.Where( a => (a.ExecuteCase == queryStatus || a.ExecuteCase == StatusType.Any) );
            Parallel.ForEach( actionList, a =>   //foreach( ActionItem a in actionList )
            {
                ExecuteResult r = executeHandlerMethod( parentSecurityContext, a, dynamicData, dryRun );
                if( a.HasActions )
                    r = ProcessRecursive( a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun, executeHandlerMethod );

                if( r.Status > returnResult.Status )
                    returnResult = r;
            } );

            return returnResult.Clone();
        }

        #region InProc
        ExecuteResult ExecuteHandlerProcessInProc(SecurityContext parentSecurityContext, ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            string parms = ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a );
            rt.Progress += rt_Progress;
            rt.LogMessage += rt_LogMessage;

            if( !WantsStopOrPause() )
            {
                HandlerStartInfo startInfo = new HandlerStartInfo( StartInfo )
                {
                    Parameters = parms,
                    IsDryRun = dryRun,
                    InstanceId = a.InstanceId
                };
                SecurityContext sc = a.HasRunAs ? a.RunAs : parentSecurityContext;
                sc?.Impersonate();
                a.Result = rt.Execute( startInfo );
                sc?.Undo();
            }

            return a.Result;
        }

        void rt_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            if( _wantsCancel ) { e.Cancel = true; }
            SynapseDal.UpdateActionInstance( e.Id, e.Status, e.Message, e.Sequence );
            OnProgress( e );
            if( e.Cancel ) { _wantsCancel = true; }
        }

        private void rt_LogMessage(object sender, LogMessageEventArgs e)
        {
            OnLogMessage( e );
        }
        #endregion


        #region External
        ExecuteResult ExecuteHandlerProcessExternal(SecurityContext parentSecurityContext, ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            if( !WantsStopOrPause() )
            {
                if( !a.HasRunAs )
                    a.RunAs = parentSecurityContext;
                a.Result = SpawnExternal( a, dynamicData, dryRun );
                return a.Result;
            }
            else
            {
                return ExecuteResult.Emtpy;
            }
        }

        private ExecuteResult SpawnExternal(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun)
        {
            ExecuteResult result = new ExecuteResult();
            List<string> args = new List<string>();

            Plan container = new Plan();
            container.Name = $"{Name}:{a.Name}";
            container.Actions.Add( a.Clone() );
            container.StartInfo = StartInfo;

            string planYaml = CrytoHelpers.Encode( container.ToYaml() );
            args.Add( $"/plan:{planYaml}" );
            args.Add( $"/dryRun:{dryRun}" );
            args.Add( $"/taskModel:single" );
            foreach( string key in dynamicData.Keys )
                args.Add( $"/{key}:{dynamicData[key]}" );
            string arguments = string.Join( " ", args );

            Process p = new Process();
            p.StartInfo.Arguments = arguments;
            p.StartInfo.FileName = "synapse.cli.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.EnableRaisingEvents = true;
            //if( a.HasRunAs )
            //{
            //    p.StartInfo.Domain = a.RunAs.Domain;
            //    p.StartInfo.UserName = a.RunAs.UserName;
            //    SecureString pw = new SecureString();
            //    foreach( char c in a.RunAs.Password )
            //        pw.AppendChar( c );
            //    p.StartInfo.Password = pw;
            //}

            p.OutputDataReceived += p_OutputDataReceived;

            p.Start();

            result.PId = p.Id;
            a.UpdateInstanceStatus( StatusType.None, "SpawnExternal", 0, p.Id );

            #region read this
            // best practice information on accessing stdout/stderr from mdsn article:
            //  https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.redirectstandardoutput%28v=vs.110%29.aspx
            // Do not wait for the child process to exit before reading to the end of its redirected stream.
            // Do not perform a synchronous read to the end of both redirected streams.
            // string output = p.StandardOutput.ReadToEnd();
            // string error = p.StandardError.ReadToEnd();
            // p.WaitForExit();
            // Use asynchronous read operations on at least one of the streams.
            #endregion
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();


            p.WaitForExit();

            result.Status = (StatusType)p.ExitCode;
            return result;
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                string data = CrytoHelpers.Decode( e.Data );
                try
                {
                    HandlerProgressCancelEventArgs args = HandlerProgressCancelEventArgs.DeserializeSimple( data );
                    OnProgress( args );
                }
                catch
                {
                    try
                    {
                        LogMessageEventArgs args = LogMessageEventArgs.DeserializeSimple( data );
                        OnLogMessage( args );
                    }
                    catch
                    {
                        throw new Exception( "Could not deserialize output into known args type" );
                    }
                }
            }
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                string data = CrytoHelpers.Decode( e.Data );
                try
                {
                    HandlerProgressCancelEventArgs args = HandlerProgressCancelEventArgs.DeserializeSimple( data );
                    OnProgress( args );
                }
                catch
                {
                    try
                    {
                        LogMessageEventArgs args = LogMessageEventArgs.DeserializeSimple( data );
                        OnLogMessage( args );
                    }
                    catch
                    {
                        throw new Exception( "Could not deserialize output into known args type" );
                    }
                }
            }
        }
        #endregion


        #region SingleAction
        public ExecuteResult ExecuteHandlerProcess_SingleAction(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            ExecuteResult returnResult = ExecuteResult.Emtpy;

            string parms = ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a );
            rt.Progress += rt_Progress;
            rt.LogMessage += rt_LogMessage;

            if( !WantsStopOrPause() )
            {
                HandlerStartInfo startInfo = new HandlerStartInfo( StartInfo )
                {
                    Parameters = parms,
                    IsDryRun = dryRun,
                    InstanceId = a.InstanceId
                };
                a.RunAs?.Impersonate();
                ExecuteResult r = rt.Execute( startInfo );
                a.RunAs?.Undo();

                if( r.Status > returnResult.Status )
                    returnResult = r;
            }

            return returnResult;
        }

        //void rt_Single_Progress(object sender, HandlerProgressCancelEventArgs e)
        //{
        //    if( _wantsCancel ) { e.Cancel = true; }
        //    (new ActionItem() { InstanceId = e.Id }).UpdateInstanceStatus( e.Status, e.Message, e.Sequence );
        //    OnProgress( e );
        //    if( e.Cancel ) { _wantsCancel = true; }
        //}
        #endregion


        #region utility methods
        string ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData)
        {
            bool cancel = OnProgress( a.Name, "ResolveConfigAndParameters", "Start", StatusType.Initializing, a.InstanceId, -2 );
            if( cancel )
            {
                _wantsCancel = true;
                return null;
            }

            if( a.Handler.HasConfig )
            {
                ParameterInfo c = a.Handler.Config;
                if( c.HasInheritFrom && _configSets.Keys.Contains( c.InheritFrom ) )
                {
                    c.InheritedValues = _configSets[c.InheritFrom];
                }
                c.Resolve( dynamicData );

                if( c.HasName )
                {
                    _configSets[c.Name] = c;
                }
            }

            string parms = null;
            if( a.HasParameters )
            {
                ParameterInfo p = a.Parameters;
                if( p.HasInheritFrom && _paramSets.Keys.Contains( p.InheritFrom ) )
                {
                    p.InheritedValues = _paramSets[p.InheritFrom];
                }
                parms = p.Resolve( dynamicData );

                if( p.HasName )
                {
                    _paramSets[p.Name] = p;
                }
            }

            return parms;
        }

        IHandlerRuntime CreateHandlerRuntime(ActionItem a)
        {
            HandlerInfo info = a.Handler;

            bool cancel = OnProgress( a.Name, "CreateHandlerRuntime: " + info.Type, "Start", StatusType.Initializing, a.InstanceId, -1 );
            if( cancel )
            {
                _wantsCancel = true;
                return new Runtime.EmptyHandler();
            }

            IHandlerRuntime hr = new Runtime.EmptyHandler();

            string[] typeInfo = info.Type.Split( ':' );
            AssemblyName an = new AssemblyName( typeInfo[0] );
            Assembly hrAsm = Assembly.Load( an );
            Type handlerRuntime = hrAsm.GetType( typeInfo[1], true );
            hr = Activator.CreateInstance( handlerRuntime ) as IHandlerRuntime;
            hr.ActionName = a.Name;

            string config = info.HasConfig ? info.Config.ResolvedValuesSerialized : null;
            hr.Initialize( config );

            return hr;
        }
        #endregion
    }
}