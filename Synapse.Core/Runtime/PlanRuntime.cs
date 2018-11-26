using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

#if sqlite
using Synapse.Core.DataAccessLayer;
#endif

using Synapse.Core.Utilities;
using YamlDotNet.Serialization;

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


        //this is a stub feature
        static long ActionInstanceIdCounter = 0;// DateTime.Now.Ticks;


        bool _wantsCancel = false;
        bool _wantsPause = false;

        Dictionary<string, ParameterInfo> _configSets = new Dictionary<string, ParameterInfo>();
        Dictionary<string, ParameterInfo> _paramSets = new Dictionary<string, ParameterInfo>();

        #region control methods
        public void Stop() { _wantsCancel = true; }
        public void Pause() { _wantsPause = true; }
        public void Continue() { _wantsPause = false; }
        [YamlIgnore]
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
#if sqlite
            SynapseDal.CreateDatabase();
            CreateInstance();
#endif

            AddStartInfoToDynamicData(ref dynamicData);

            ResultPlan = new Plan
            {
                Name = Name,
                UniqueName = UniqueName,
                Description = Description,
                InstanceId = InstanceId,
                StartInfo = StartInfo,
                Signature = Signature,
                RunAs = RunAs,
                Crypto = Crypto,
                Result = new ExecuteResult()
            };

            if( inProc )
                ProcessRecursive( ResultPlan, RunAs, null, Actions, ResultPlan.Result, dynamicData, dryRun, ExecuteHandlerProcessInProc );
            else
                ProcessRecursive( ResultPlan, RunAs, null, Actions, ResultPlan.Result, dynamicData, dryRun, ExecuteHandlerProcessExternal );

            ResultPlan.Result.Status = ResultPlan.Result.BranchStatus;
            Result = ResultPlan.Result;

#if sqlite
            UpdateInstanceStatus( Result.Status, Result.Status.ToString() );
#endif

            return Result;
        }

        /// <summary>
        /// Adds seelcted values to global DynamicData parms dict, ref for clarity that it mutates the values
        /// </summary>
        /// <param name="dynamicData"></param>
        void AddStartInfoToDynamicData(ref Dictionary<string, string> dynamicData)
        {
            if( dynamicData == null )
                dynamicData = new Dictionary<string, string>();

            string name = "PlanStartInfo_";
            dynamicData[$"{name}{nameof( Name )}"] = Name;
            dynamicData[$"{name}{nameof( UniqueName )}"] = UniqueName;
            dynamicData[$"{name}{nameof( IsActive )}"] = IsActive.ToString();
            dynamicData[$"{name}{nameof( InstanceId )}"] = InstanceId.ToString();
            dynamicData[$"{name}{nameof( StartInfo.RequestNumber )}"] = StartInfo.RequestNumber;
            dynamicData[$"{name}{nameof( StartInfo.RequestUser )}"] = StartInfo.RequestUser;
        }

        void ProcessRecursive(IActionContainer parentContext, SecurityContext parentSecurityContext,
            ActionItem actionGroup, List<ActionItem> actions, ExecuteResult parentResult,
            Dictionary<string, string> dynamicData, bool dryRun,
            Func<SecurityContext, ActionItem, Dictionary<string, string>, object, bool, ExecuteResult> executeHandlerMethod)
        {
            if( WantsStopOrPause() )
                return;


            parentContext.EnsureInitialized();

            //queryStatus provides the inbound value to ExecuteCase evaluation; it's the ExecuteCase comparison value.
            //queryStatus is carried downward through successive subtree executions.
            StatusType queryStatus = parentResult.Status;

            #region actionGroup
            if( actionGroup != null &&
                (((actionGroup.ExecuteCase & parentResult.Status) == actionGroup.ExecuteCase) || (actionGroup.ExecuteCase == StatusType.Any)) )
            {
                actionGroup.EnsureInitialized();

                #region actionGroup-forEach
                if( (actionGroup.HasParameters && actionGroup.Parameters.HasForEach) ||
                    (actionGroup.Handler.HasConfig && actionGroup.Handler.Config.HasForEach) )
                {
                    List<ActionItem> resolvedParmsActionGroup = new List<ActionItem>();
                    ResolveConfigAndParameters( actionGroup, dynamicData, ref resolvedParmsActionGroup, parentResult.ExitData );

                    foreach( ActionItem ai in resolvedParmsActionGroup )
                        ai.Parameters.ForEach = null;

                    actionGroup.InstanceId = ActionInstanceIdCounter++;
                    ActionItem agclone = actionGroup.Clone();
                    agclone.Result = new ExecuteResult();
                    parentContext.ActionGroup = agclone;

                    Parallel.ForEach( resolvedParmsActionGroup, a =>   // foreach( ActionItem a in resolvedParmsActionGroup )
                    {
#if sqlite
                        a.CreateInstance( parentContext, InstanceId );
#endif
                        a.InstanceId = ActionInstanceIdCounter++;
                        ActionItem clone = a.Clone();
                        agclone.Actions.Add( clone );

                        ExecuteResult r = executeHandlerMethod( parentSecurityContext, a, dynamicData, parentResult.ExitData, dryRun );
                        parentContext.ActionGroup.Result.SetBranchStatusChecked( r );
                        parentContext.Result.SetBranchStatusChecked( r );
                        clone.Handler.Type = actionGroup.Handler.Type;
                        clone.Handler.StartInfo = actionGroup.Handler.StartInfo;
                        clone.Result = r;

                        if( r.Status > queryStatus ) queryStatus = r.Status;

                        if( a.HasActions )
                        {
                            ProcessRecursive( clone, a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun, executeHandlerMethod );
                            parentContext.ActionGroup.Result.SetBranchStatusChecked( clone.Result );
                            parentContext.Result.SetBranchStatusChecked( clone.Result );

                            if( clone.Result.Status > queryStatus ) queryStatus = clone.Result.Status;
                        }
                    } );
                }
                #endregion
                #region actionGroup-single
                else
                {
#if sqlite
                    actionGroup.CreateInstance( parentContext, InstanceId );
#endif
                    ResolveConfigAndParameters( actionGroup, dynamicData, parentResult.ExitData );

                    actionGroup.InstanceId = ActionInstanceIdCounter++;
                    ActionItem clone = actionGroup.Clone();
                    parentContext.ActionGroup = clone;

                    ExecuteResult r = executeHandlerMethod( parentSecurityContext, actionGroup, dynamicData, parentResult.ExitData, dryRun );
                    parentContext.Result.SetBranchStatusChecked( r );
                    clone.Handler.Type = actionGroup.Handler.Type;
                    clone.Handler.StartInfo = actionGroup.Handler.StartInfo;
                    clone.Result = r;

                    if( r.Status > queryStatus ) queryStatus = r.Status;

                    if( actionGroup.HasActions )
                    {
                        ProcessRecursive( clone, actionGroup.RunAs, null, actionGroup.Actions, r, dynamicData, dryRun, executeHandlerMethod );
                        parentContext.Result.SetBranchStatusChecked( clone.Result );

                        if( clone.Result.Status > queryStatus ) queryStatus = clone.Result.Status;
                    }
                }
                #endregion
            }
            #endregion


            #region actions
            IEnumerable<ActionItem> actionList =
                actions.Where( a => (((a.ExecuteCase & queryStatus) == a.ExecuteCase) || (a.ExecuteCase == StatusType.Any)) );

            List<ActionItem> resolvedParmsActions = new List<ActionItem>();
            Parallel.ForEach( actionList, a =>   // foreach( ActionItem a in actionList )
                ResolveConfigAndParameters( a, dynamicData, ref resolvedParmsActions, parentResult.ExitData )
            );

            Parallel.ForEach( resolvedParmsActions, a =>   // foreach( ActionItem a in resolvedParmsActions )
            {
#if sqlite
                a.CreateInstance( parentContext, InstanceId );
#endif
                a.InstanceId = ActionInstanceIdCounter++;
                ActionItem clone = a.Clone();
                parentContext.Actions.Add( clone );

                ExecuteResult r = a.Result;
                if( r?.Status != StatusType.Failed )
                    r = executeHandlerMethod( parentSecurityContext, a, dynamicData, parentResult.ExitData, dryRun );

                parentContext.Result.SetBranchStatusChecked( r );
                clone.Handler.Type = a.Handler.Type;
                clone.Handler.StartInfo = a.Handler.StartInfo;
                clone.Result = r;

                if( a.HasActions )
                {
                    ProcessRecursive( clone, a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun, executeHandlerMethod );
                    parentContext.Result.SetBranchStatusChecked( clone.Result );
                }
            } );
            #endregion
        }


        #region InProc
        ExecuteResult ExecuteHandlerProcessInProc(SecurityContext parentSecurityContext, ActionItem a,
            Dictionary<string, string> dynamicData, object parentExitData, bool dryRun = false)
        {
            try
            {
                //string parms = ResolveConfigAndParameters( a, dynamicData );
                string parms = a.Parameters.GetSerializedValues( Crypto, out string safeSerializedValues );

                IHandlerRuntime rt = CreateHandlerRuntime( a );
                rt.Progress += rt_Progress;
                rt.LogMessage += rt_LogMessage;

                if( !WantsStopOrPause() )
                {
                    #region  prep the Handler parms
                    HandlerStartInfo startInfo = new HandlerStartInfo( StartInfo )
                    {
                        Parameters = parms,
                        IsDryRun = dryRun,
                        PlanInstanceId = InstanceId,
                        InstanceId = a.InstanceId,
                        ParentExitData = parentExitData,
                        RunAs = a.RunAs,
                        Crypto = a.Parameters?.Crypto
                    };
                    //startInfoData serializes less than full HandlerStartInfo (avoids Crypto/RunAs)
                    a.Handler.StartInfo = new HandlerStartInfoData( startInfo );
                    #endregion

                    #region SecurityContext
                    ISecurityContextRuntime scr = null;

                    a.IngestParentSecurityContext( parentSecurityContext );
                    if( a.HasRunAs && a.RunAs.IsValid )
                    {
                        string securityContextParms = a.RunAs.Parameters?.GetSerializedValues( Crypto, out string safeSerializedsecurityContextValues );
                        SecurityContextStartInfo securityContextStartInfo = new SecurityContextStartInfo( StartInfo )
                        {
                            Parameters = securityContextParms,
                            IsDryRun = dryRun
                        };

                        scr = CreateSecurityContextRuntime( a );
                        ExecuteResult logonResult = scr?.Logon( securityContextStartInfo );
                        if( logonResult?.Status == StatusType.Failed )
                            throw new Exception( logonResult.Message );
                    }
                    #endregion

                    a.Result = rt.Execute( startInfo );

                    a.Handler.StartInfo.Parameters = safeSerializedValues; //avoids serializing decrypted values to the History Plan (bug #93)
                    a.Result.BranchStatus = a.Result.Status;
                    a.Result.SecurityContext = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    SaveExitDataAs( a );

                    scr?.Logoff();
                }

                return a.Result;
            }
            catch( Exception ex )
            {
                WriteUnhandledActionException( a, ex );
                return new ExecuteResult() { Status = StatusType.Failed, ExitData = ex.Message };
            }
        }
        #endregion


        #region External
        ExecuteResult ExecuteHandlerProcessExternal(SecurityContext parentSecurityContext, ActionItem a,
            Dictionary<string, string> dynamicData, object parentExitData, bool dryRun = false)
        {
            if( !WantsStopOrPause() )
            {
                try
                {
                    //if( !a.HasRunAs ) a.RunAs = parentSecurityContext;
                    a.IngestParentSecurityContext( parentSecurityContext );
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
                return new ExecuteResult();
            }
        }

        //note: this doesn't implement SaveExitDataAs( action );
        private ExecuteResult SpawnExternal(ActionItem a, Dictionary<string, string> dynamicData, object parentExitData, bool dryRun)
        {
            ExecuteResult result = new ExecuteResult();
            List<string> args = new List<string>();

            Plan container = new Plan();
            a.InstanceId = ActionInstanceIdCounter++;
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
#if sqlite
            a.UpdateInstanceStatus( StatusType.None, "SpawnExternal", 0, p.Id );
#endif
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

            result.BranchStatus = result.Status = (StatusType)p.ExitCode;
            return result;
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                string data = CryptoHelpers.Decode( e.Data );
                try
                {
                    HandlerProgressCancelEventArgs args = HandlerProgressCancelEventArgs.DeserializeSimple( data, true );
                    OnProgress( args );
                }
                catch
                {
                    try
                    {
                        LogMessageEventArgs args = LogMessageEventArgs.DeserializeSimple( data, true );
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
                    HandlerProgressCancelEventArgs args = HandlerProgressCancelEventArgs.DeserializeSimple( data, true );
                    OnProgress( args );
                }
                catch
                {
                    try
                    {
                        LogMessageEventArgs args = LogMessageEventArgs.DeserializeSimple( data, true );
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
            ExecuteResult returnResult = new ExecuteResult();

            ResolveConfigAndParameters( a, dynamicData, parentExitData );

            IHandlerRuntime rt = CreateHandlerRuntime( a );
            rt.Progress += rt_Progress;
            rt.LogMessage += rt_LogMessage;

            if( !WantsStopOrPause() )
            {
                try
                {
                    HandlerStartInfo startInfo = new HandlerStartInfo( StartInfo )
                    {
                        Parameters = a.Parameters.GetSerializedValues( Crypto ),
                        IsDryRun = dryRun,
                        PlanInstanceId = InstanceId,
                        InstanceId = a.InstanceId,
                        ParentExitData = parentExitData
                    };

                    ISecurityContextRuntime scr = CreateSecurityContextRuntime( a );
                    string securityContextParms = a.Parameters.GetSerializedValues( Crypto, out string safeSerializedsecurityContextValues );
                    SecurityContextStartInfo securityContextStartInfo = new SecurityContextStartInfo( StartInfo )
                    {
                        Parameters = securityContextParms,
                        IsDryRun = dryRun,
                        //Crypto = a.RunAs?.Crypto
                    };
                    scr?.Logon( securityContextStartInfo );

                    ExecuteResult r = rt.Execute( startInfo );

                    SaveExitDataAs( a );

                    scr?.Logoff();

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
#if sqlite
            SynapseDal.UpdateActionInstance( e.Id, e.Status, e.Message, e.Sequence );
#endif
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
            string message = $"An unhandled exeption occurred in {a.Name}, Plan/Action Instance: {InstanceId}/{a.InstanceId}. Message: {ex.Message}.";

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
        void ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData, object parentExitData)
        {
            bool cancel = OnProgress( a.Name, "ResolveConfigAndParameters", "Start", StatusType.Initializing, a.InstanceId, -2 );
            if( cancel )
            {
                _wantsCancel = true;
                return;
            }

            try
            {
                List<ActionItem> resolvedActions = null;
                a.ResolveConfigAndParameters( dynamicData, _configSets, _paramSets, ref resolvedActions, parentExitData );
            }
            catch( Exception ex )
            {
                if( !a.HasResult ) { a.Result = new ExecuteResult(); }

                a.Result.Status = a.Result.BranchStatus = StatusType.Failed;
                a.Result.Message = $"Exception in ResolveConfigAndParameters: [{ex.Message}]";

                OnProgress( a.Name, "ResolveConfigAndParameters", ex.Message, StatusType.Failed, a.InstanceId, -2 );
                throw;
            }
        }
        void ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData, ref List<ActionItem> resolvedActions, object parentExitData)
        {
            bool cancel = OnProgress( a.Name, "ResolveConfigAndParameters", "Start", StatusType.Initializing, a.InstanceId, -2 );
            if( cancel )
            {
                _wantsCancel = true;
                return;
            }

            try
            {
                a.ResolveConfigAndParameters( dynamicData, _configSets, _paramSets, ref resolvedActions, parentExitData );
            }
            catch( Exception ex )
            {
                if( !a.HasResult ) { a.Result = new ExecuteResult(); }

                string message = ExceptionHelpers.UnwindException( "ResolveConfigAndParameters", ex, asSingleLine: false );

                a.Result.Status = a.Result.BranchStatus = StatusType.Failed;
                a.Result.Message = $"Exception in ResolveConfigAndParameters: [{message}]";

                OnProgress( a.Name, "ResolveConfigAndParameters", ex.Message, StatusType.Failed, a.InstanceId, -2 );

                resolvedActions.Add( a );
            }
        }

        IHandlerRuntime CreateHandlerRuntime(ActionItem a)
        {
            HandlerInfo handler = a.Handler;

            bool cancel = OnProgress( a.Name, "CreateHandlerRuntime: " + handler.Type, "Start", StatusType.Initializing, a.InstanceId, -1 );
            if( cancel )
            {
                _wantsCancel = true;
                return new EmptyHandler();
            }

            IHandlerRuntime hr = AssemblyLoader.Load( handler.Type, DefaultHandlerType );

            if( hr != null )
            {
                handler.Type = hr.RuntimeType;
                hr.ActionName = a.Name;

                string config = handler.HasConfig ? handler.Config.GetSerializedValues( Crypto ) : null;
                hr.Initialize( config );
            }
            else
            {
                throw new Exception( $"Could not load {handler.Type}." );
            }

            return hr;
        }

        ISecurityContextRuntime CreateSecurityContextRuntime(ActionItem a)
        {
            SecurityContext securityContext = a.RunAs;
            if( securityContext == null )
                return null;

            bool cancel = OnProgress( a.Name, "CreateHandlerRuntime: " + securityContext.Type, "Start", StatusType.Initializing, a.InstanceId, -1 );
            if( cancel )
            {
                _wantsCancel = true;
                return null;
            }

            ISecurityContextRuntime scr = AssemblyLoader.Load<ISecurityContextRuntime>( securityContext.Type, "Synapse.Handlers.SecurityContext:Win32Impersonator" );

            if( scr != null )
            {
                securityContext.Type = scr.RuntimeType;
                scr.ActionName = a.Name;

                string config = securityContext.HasConfig ? securityContext.Config.GetSerializedValues( Crypto ) : null;
                scr.Initialize( config );
            }
            else
            {
                throw new Exception( $"Could not load {securityContext.Type}." );
            }

            return scr;
        }

        void SaveExitDataAs(ActionItem a)
        {
            if( a.HasSaveExitDataAs )
            {
                if( a.SaveExitDataAs.HasConfig )
                    _configSets[a.SaveExitDataAs.Config] = new ParameterInfo() { Values = a.Result.ExitData };
                if( a.SaveExitDataAs.HasParaamters )
                    _paramSets[a.SaveExitDataAs.Parameters] = new ParameterInfo() { Values = a.Result.ExitData };
            }
        }
        #endregion
    }
}