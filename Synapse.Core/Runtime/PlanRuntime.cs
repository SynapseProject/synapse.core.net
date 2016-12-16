using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            if( _wantsCancel )
                e.Cancel = true;

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

            ResultPlan = new Plan();
            ResultPlan.InstanceId = InstanceId;

            if( inProc )
                Result = ProcessRecursive( ResultPlan, RunAs, null, Actions, ExecuteResult.Emtpy, dynamicData, dryRun, ExecuteHandlerProcessInProc );
            else
                Result = ProcessRecursive( ResultPlan, RunAs, null, Actions, ExecuteResult.Emtpy, dynamicData, dryRun, ExecuteHandlerProcessExternal );

            UpdateInstanceStatus( Result.Status, Result.Status.ToString() );

            ResultPlan.Result = Result;
            return Result;
        }

        ExecuteResult ProcessRecursive(IActionContainer parentContext, SecurityContext parentSecurityContext,
            ActionItem actionGroup, List<ActionItem> actions, ExecuteResult result,
            Dictionary<string, string> dynamicData, bool dryRun,
            Func<SecurityContext, ActionItem, Dictionary<string, string>, string, bool, ExecuteResult> executeHandlerMethod)
        {
            if( WantsStopOrPause() ) { return result; }

            ExecuteResult returnResult = ExecuteResult.Emtpy;

            //queryStatus provides the inbound value to ExecuteCase evaluation; it's the ExecuteCase comparison value.
            //queryStatus is carried downward through successive subtree executions.
            StatusType queryStatus = result.Status;

            #region actionGroup
            if( actionGroup != null && (actionGroup.ExecuteCase == result.Status || actionGroup.ExecuteCase == StatusType.Any) )
            {
                if( actionGroup.Handler == null )
                    actionGroup.Handler = new HandlerInfo();

                #region actionGroup-forEach
                if( (actionGroup.HasParameters && actionGroup.Parameters.HasForEach) ||
                    (actionGroup.Handler.HasConfig && actionGroup.Handler.Config.HasForEach) )
                {
                    //tracks the local status of this ActionGroup ForEach branch
                    StatusType agForEachBranchStatus = StatusType.None;

                    List<ActionItem> resolvedParmsActionGroup = new List<ActionItem>();
                    ResolveConfigAndParameters( actionGroup, dynamicData, ref resolvedParmsActionGroup );
                    foreach( ActionItem ai in resolvedParmsActionGroup )
                        ai.Parameters.ForEach = null;

                    ActionItem agclone = actionGroup.Clone();
                    parentContext.ActionGroup = agclone;

                    //Parallel.ForEach( resolvedParmsActionGroup, a =>   //
                    foreach( ActionItem a in resolvedParmsActionGroup )
                    {
                        a.CreateInstance( parentContext, InstanceId );

                        ActionItem clone = a.Clone();
                        agclone.Actions.Add( clone );

                        ExecuteResult r = executeHandlerMethod( parentSecurityContext, a, dynamicData, result.ExitData, dryRun );
                        clone.Handler.Type = actionGroup.Handler.Type;
                        clone.Result = r;

                        if( r.Status > queryStatus ) queryStatus = r.Status;
                        if( r.Status > agForEachBranchStatus ) agForEachBranchStatus = r.Status;

                        if( a.HasActions )
                        {
                            r = ProcessRecursive( clone, a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun, executeHandlerMethod );

                            if( r.Status > queryStatus ) queryStatus = r.Status;
                            if( r.Status > agForEachBranchStatus ) agForEachBranchStatus = r.Status;
                        }
                    } //);

                    agclone.Result = new ExecuteResult() { Status = StatusType.None, BranchStatus = agForEachBranchStatus };

                    if( queryStatus > returnResult.Status ) returnResult.Status = queryStatus;
                }
                #endregion
                #region actionGroup-single
                else
                {
                    //tracks the local status of this ActionGroup branch
                    StatusType agSingleBranchStatus = StatusType.None;

                    actionGroup.CreateInstance( parentContext, InstanceId );

                    ActionItem clone = actionGroup.Clone();
                    parentContext.ActionGroup = clone;

                    ExecuteResult r = executeHandlerMethod( parentSecurityContext, actionGroup, dynamicData, result.ExitData, dryRun );
                    clone.Handler.Type = actionGroup.Handler.Type;
                    clone.Result = r;

                    if( r.Status > returnResult.Status ) returnResult = r;
                    if( r.Status > agSingleBranchStatus ) agSingleBranchStatus = r.Status;

                    if( actionGroup.HasActions )
                    {
                        r = ProcessRecursive( clone, parentSecurityContext, null, actionGroup.Actions, r, dynamicData, dryRun, executeHandlerMethod );

                        if( r.Status > returnResult.Status ) returnResult = r;
                        if( r.Status > agSingleBranchStatus ) agSingleBranchStatus = r.Status;
                    }

                    if( r.Status > queryStatus ) queryStatus = r.Status;

                    clone.Result.BranchStatus = agSingleBranchStatus;
                }
                #endregion
            }
            #endregion


            #region actions
            IEnumerable<ActionItem> actionList =
                actions.Where( a => (a.ExecuteCase == queryStatus || a.ExecuteCase == StatusType.Any) );

            List<ActionItem> resolvedParmsActions = new List<ActionItem>();
            Parallel.ForEach( actionList, a =>   // foreach( ActionItem a in actionList )
                ResolveConfigAndParameters( a, dynamicData, ref resolvedParmsActions )
            );

            //tracks the local status of this subtree branch
            StatusType actionSubtreeBranchStatus = StatusType.None;

            //Parallel.ForEach( resolvedParmsActions, a =>   //
            foreach( ActionItem a in resolvedParmsActions )
            {
                a.CreateInstance( parentContext, InstanceId );

                ActionItem clone = a.Clone();
                parentContext.Actions.Add( clone );

                ExecuteResult r = executeHandlerMethod( parentSecurityContext, a, dynamicData, result.ExitData, dryRun );
                clone.Handler.Type = a.Handler.Type;
                clone.Result = r;

                if( r.Status > returnResult.Status ) returnResult = r;
                if( r.Status > actionSubtreeBranchStatus ) actionSubtreeBranchStatus = r.Status;

                if( a.HasActions )
                {
                    r = ProcessRecursive( clone, a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun, executeHandlerMethod );

                    if( r.Status > returnResult.Status ) returnResult = r;
                    if( r.Status > actionSubtreeBranchStatus ) actionSubtreeBranchStatus = r.Status;
                }
            } //);
            #endregion

            if( parentContext.Result == null )
                parentContext.Result = new ExecuteResult();

            if( actionSubtreeBranchStatus > parentContext.Result.BranchStatus )
                parentContext.Result.BranchStatus = actionSubtreeBranchStatus;

            returnResult.BranchStatus = returnResult.Status;
            return returnResult.Clone();
        }


        #region InProc
        ExecuteResult ExecuteHandlerProcessInProc(SecurityContext parentSecurityContext, ActionItem a,
            Dictionary<string, string> dynamicData, string parentExitData, bool dryRun = false)
        {
            try
            {
                //string parms = ResolveConfigAndParameters( a, dynamicData );
                string parms = a.Parameters.GetSerializedValues();

                IHandlerRuntime rt = CreateHandlerRuntime( a );
                rt.Progress += rt_Progress;
                rt.LogMessage += rt_LogMessage;

                if( !WantsStopOrPause() )
                {
                    HandlerStartInfo startInfo = new HandlerStartInfo( StartInfo )
                    {
                        Parameters = parms,
                        IsDryRun = dryRun,
                        InstanceId = a.InstanceId,
                        ParentExitData = parentExitData
                    };
                    SecurityContext sc = a.HasRunAs ? a.RunAs : parentSecurityContext;
                    sc?.Impersonate();
                    a.Result = rt.Execute( startInfo );
                    a.Result.BranchStatus = a.Result.Status;
                    sc?.Undo();
                }

                return a.Result;
            }
            catch( Exception ex )
            {
                WriteUnhandledActionException( a, ex );
                return new ExecuteResult() { Status = StatusType.Failed };
            }
        }
        #endregion


        #region External
        ExecuteResult ExecuteHandlerProcessExternal(SecurityContext parentSecurityContext, ActionItem a,
            Dictionary<string, string> dynamicData, string parentExitData, bool dryRun = false)
        {
            if( !WantsStopOrPause() )
            {
                try
                {
                    if( !a.HasRunAs )
                        a.RunAs = parentSecurityContext;
                    a.Result = SpawnExternal( a, dynamicData, parentExitData, dryRun );
                    return a.Result;
                }
                catch( Exception ex )
                {
                    WriteUnhandledActionException( a, ex );
                    return new ExecuteResult() { Status = StatusType.Failed };
                }
            }
            else
            {
                return ExecuteResult.Emtpy;
            }
        }

        private ExecuteResult SpawnExternal(ActionItem a, Dictionary<string, string> dynamicData, object parentExitData, bool dryRun)
        {
            ExecuteResult result = new ExecuteResult();
            List<string> args = new List<string>();

            Plan container = new Plan();
            container.Name = $"{Name}:{a.Name}";
            container.Actions.Add( a.Clone() );
            container.StartInfo = StartInfo;

            string planYaml = CryptoHelpers.Encode( container.ToYaml() );
            args.Add( $"/plan:{planYaml}" );
            args.Add( $"/dryRun:{dryRun}" );
            args.Add( $"/taskModel:single" );
            foreach( string key in dynamicData.Keys )
                args.Add( $"/{key}:{dynamicData[key]}" );
            args.Add( $"/data:{parentExitData}" );
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
                string data = CryptoHelpers.Decode( e.Data );
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
                        throw new Exception( $"Could not deserialize output into known args type:\r\n{data}" );
                    }
                }
            }
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                string data = CryptoHelpers.Decode( e.Data );
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
        public ExecuteResult ExecuteHandlerProcess_SingleAction(ActionItem a, Dictionary<string, string> dynamicData, string parentExitData, bool dryRun = false)
        {
            ExecuteResult returnResult = ExecuteResult.Emtpy;

            ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a );
            rt.Progress += rt_Progress;
            rt.LogMessage += rt_LogMessage;

            if( !WantsStopOrPause() )
            {
                try
                {
                    HandlerStartInfo startInfo = new HandlerStartInfo( StartInfo )
                    {
                        Parameters = a.Parameters.GetSerializedValues(),
                        IsDryRun = dryRun,
                        InstanceId = a.InstanceId,
                        ParentExitData = parentExitData
                    };
                    a.RunAs?.Impersonate();
                    ExecuteResult r = rt.Execute( startInfo );
                    a.RunAs?.Undo();

                    if( r.Status > returnResult.Status )
                        returnResult = r;
                }
                catch( Exception ex )
                {
                    WriteUnhandledActionException( a, ex );
                    returnResult = new ExecuteResult() { Status = StatusType.Failed };
                }
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


        #region progress/log handlers
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

        void WriteUnhandledActionException(ActionItem a, Exception ex, bool writeProgress = true, bool writeLog = true)
        {
            //todo: serialize exception to composite string
            string context = "Synapse.Core PlanRuntime";
            string message = $"An unhandled exeption occurred in {a.Name}, Plan/Action Instance: {a.PlanInstanceId}/{a.InstanceId}. Message: {ex.Message}.";

            if( writeProgress )
            {
                HandlerProgressCancelEventArgs e =
                    new HandlerProgressCancelEventArgs( context: context, message: message, status: StatusType.Failed,
                    id: a.InstanceId, sequence: Int32.MaxValue - 100, cancel: false, ex: ex )
                    {
                        ActionName = a.Name
                    };
                rt_Progress( this, e );
            }

            if( writeLog )
            {
                LogMessageEventArgs e =
                    new LogMessageEventArgs( context: context, message: message, level: LogLevel.Fatal, ex: ex );
                rt_LogMessage( this, e );
            }
        }
        #endregion


        #region utility methods
        void ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData)
        {
            List<ActionItem> resolvedActions = null;
            a.ResolveConfigAndParameters( dynamicData, _configSets, _paramSets, ref resolvedActions );
        }
        void ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData, ref List<ActionItem> resolvedActions)
        {
            bool cancel = OnProgress( a.Name, "ResolveConfigAndParameters", "Start", StatusType.Initializing, a.InstanceId, -2 );
            if( cancel )
            {
                _wantsCancel = true;
                return;
            }

            a.ResolveConfigAndParameters( dynamicData, _configSets, _paramSets, ref resolvedActions );
        }

        IHandlerRuntime CreateHandlerRuntime(ActionItem a)
        {
            HandlerInfo handler = a.Handler;

            bool cancel = OnProgress( a.Name, "CreateHandlerRuntime: " + handler.Type, "Start", StatusType.Initializing, a.InstanceId, -1 );
            if( cancel )
            {
                _wantsCancel = true;
                return new Runtime.EmptyHandler();
            }

            IHandlerRuntime hr = AssemblyLoader.Load( handler.Type, DefaultHandlerType );

            if( hr != null )
            {
                handler.Type = hr.RuntimeType;
                hr.ActionName = a.Name;

                string config = handler.HasConfig ? handler.Config.GetSerializedValues() : null;
                hr.Initialize( config );
            }
            else
            {
                throw new Exception( $"Could not load {handler.Type}." );
            }

            return hr;
        }
        #endregion
    }
}