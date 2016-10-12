using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public partial class Plan
    {
        #region events
        public event EventHandler<HandlerProgressCancelEventArgs> Progress;

        /// <summary>
        /// Notify of step start. If return value is True, then cancel operation.
        /// </summary>
        /// <param name="context">The method name or workflow activty.</param>
        /// <param name="message">Descriptive message.</param>
        /// <param name="status">Overall Package status indicator.</param>
        /// <param name="id">Message Id.</param>
        /// <param name="severity">Message/error severity.</param>
        /// <param name="ex">Current exception (optional).</param>
        /// <returns>HandlerProgressCancelEventArgs.Cancel value.</returns>
        protected virtual bool OnProgress(string actionName, string context, string message, StatusType status = StatusType.Running, int id = 0, int severity = 0, bool cancel = false, Exception ex = null)
        {
            HandlerProgressCancelEventArgs e =
                new HandlerProgressCancelEventArgs( context, message, status, id, severity, cancel, ex ) { ActionName = actionName };
            if( _wantsCancel ) { e.Cancel = true; }
            OnProgress( e );

            return e.Cancel;
        }

        /// <summary>
        /// Notify of step start. If e.Cancel is True, then cancel operation.
        /// </summary>
        protected virtual void OnProgress(HandlerProgressCancelEventArgs e)
        {
            if( Progress != null )
            {
                Progress( this, e );
            }
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

        public HandlerResult Start(Dictionary<string, string> dynamicData, bool dryRun = false, bool inProc = true)
        {
            if( inProc )
                return ProcessRecursive( RunAs, null, Actions, HandlerResult.Emtpy, dynamicData, dryRun, ExecuteHandlerProcessInProc );
            else
                return ProcessRecursive( RunAs, null, Actions, HandlerResult.Emtpy, dynamicData, dryRun, ExecuteHandlerProcessExternal );
        }

        HandlerResult ProcessRecursive(SecurityContext parentSecurityContext, ActionItem actionGroup, List<ActionItem> actions, HandlerResult result,
            Dictionary<string, string> dynamicData, bool dryRun,
            Func<SecurityContext, ActionItem, Dictionary<string, string>, bool, HandlerResult> executeHandlerMethod)
        {
            if( WantsStopOrPause() ) { return result; }

            HandlerResult returnResult = HandlerResult.Emtpy;

            StatusType queryStatus = result.Status;
            if( actionGroup != null && actionGroup.ExecuteCase == result.Status )
            {
                HandlerResult r = executeHandlerMethod( parentSecurityContext, actionGroup, dynamicData, dryRun );
                if( r.Status > returnResult.Status )
                    returnResult = r;

                if( actionGroup.HasActions )
                    r = ProcessRecursive( parentSecurityContext, null, actionGroup.Actions, r, dynamicData, dryRun, executeHandlerMethod );
                if( r.Status > returnResult.Status )
                    returnResult = r;

                if( r.Status > queryStatus )
                    queryStatus = r.Status;
            }

            IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == queryStatus );
            Parallel.ForEach( actionList, a =>
            {
                HandlerResult r = executeHandlerMethod( parentSecurityContext, a, dynamicData, dryRun );
                if( a.HasActions )
                    r = ProcessRecursive( a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun, executeHandlerMethod );

                if( r.Status > returnResult.Status )
                    returnResult = r;
            } );

            return returnResult;
        }

        #region InProc
        HandlerResult ExecuteHandlerProcessInProc(SecurityContext parentSecurityContext, ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            HandlerResult returnResult = HandlerResult.Emtpy;

            string parms = ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a.Name, a.Handler );
            rt.Progress += rt_Progress;

            if( !WantsStopOrPause() )
            {
                SecurityContext sc = a.HasRunAs ? a.RunAs : parentSecurityContext;
                sc?.Impersonate();
                HandlerResult r = rt.Execute( parms, dryRun );
                sc?.Undo();

                if( r.Status > returnResult.Status )
                    returnResult = r;
            }

            return returnResult;
        }

        void rt_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            if( _wantsCancel ) { e.Cancel = true; }
            OnProgress( e );
            if( e.Cancel ) { _wantsCancel = true; }
        }
        #endregion


        #region External
        HandlerResult ExecuteHandlerProcessExternal(SecurityContext parentSecurityContext, ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            HandlerResult returnResult = HandlerResult.Emtpy;

            if( !WantsStopOrPause() )
            {
                if( !a.HasRunAs )
                    a.RunAs = parentSecurityContext;
                HandlerResult r = SpawnExternal( a, dynamicData, dryRun );

                if( r.Status > returnResult.Status )
                    returnResult = r;
            }

            return returnResult;
        }

        private HandlerResult SpawnExternal(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun)
        {
            List<string> args = new List<string>();

            Plan container = new Plan();
            container.Name = $"{this.Name}:{a.Name}";
            container.Actions.Add( a.Clone() );

            string planYaml = CrytoHelpers.Encode( container.ToYaml() );
            args.Add( $"/plan:{planYaml}" );
            args.Add( $"/dryRun:{dryRun}" );
            args.Add( $"/thread:single" );
            foreach( string key in dynamicData.Keys )
                args.Add( $"/{key}:{dynamicData[key]}" );
            string arguments = string.Join( " ", args );

            //OnProgress( $" --> external --> {container.Name}", "external", arguments );
            Console.WriteLine( $" --> external --> {container.Name}" );


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

            int exitCode = p.ExitCode;
            HandlerResult result = new HandlerResult();
            result.Status = (StatusType)exitCode;
            return result;
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                OnProgress( "a", "o", e.Data );
            }
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                OnProgress( "a", "e", e.Data );
            }
        }

        public HandlerResult ExecuteHandlerProcess_SingleAction(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            HandlerResult returnResult = HandlerResult.Emtpy;

            string parms = ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a.Name, a.Handler );
            rt.Progress += rt_Progress;

            if( !WantsStopOrPause() )
            {
                a.RunAs?.Impersonate();
                HandlerResult r = rt.Execute( parms, dryRun );
                a.RunAs?.Undo();

                if( r.Status > returnResult.Status )
                    returnResult = r;
            }

            return returnResult;
        }
        #endregion


        #region utility methods
        string ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData)
        {
            bool cancel = OnProgress( a.Name, "ResolveConfigAndParameters", "Start" );
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

        IHandlerRuntime CreateHandlerRuntime(string actionName, HandlerInfo info)
        {
            bool cancel = OnProgress( actionName, "CreateHandlerRuntime: " + info.Type, "Start" );
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
            hr.ActionName = actionName;

            string config = info.HasConfig ? info.Config.ResolvedValuesSerialized : null;
            hr.Initialize( config );

            return hr;
        }
        #endregion
    }
}