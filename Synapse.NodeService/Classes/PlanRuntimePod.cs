using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Synapse.Common;
using Synapse.Core;
using Synapse.Core.Runtime;

namespace Synapse.Services
{
    public class PlanRuntimePod : IPlanRuntimeContainer
    {
        LogUtility _log = new LogUtility();
        DirectoryInfo _logRootPath = null;
        bool _wantsCancel = false;
        string _ticks = null;

        public PlanRuntimePod(Plan plan, bool isDryRun = false, Dictionary<string, string> dynamicData = null, int planInstanceId = 0)
        {
            Plan = plan;
            IsDryRun = isDryRun;
            DynamicData = dynamicData;
            PlanInstanceId = planInstanceId;

            _ticks = $"{planInstanceId}_{DateTime.Now.Ticks}";

            InitializeLogger();

            Plan.Progress += Plan_Progress;
            Plan.LogMessage += Plan_LogMessage;
        }

        public void InitializeLogger()
        {
            string logFilePath = null;
            try
            {
                string logFileName = $"{_ticks}_{Plan.Name}";
                _logRootPath = Directory.CreateDirectory( SynapseNodeService.Config.GetResolvedAuditLogRootPath() );
                logFilePath = $"{_logRootPath.FullName}\\{logFileName}.log";
                _log.InitDynamicFileAppender( logFileName, logFileName, logFilePath, SynapseNodeService.Config.Log4NetConversionPattern, "all" );
            }
            catch( Exception ex )
            {
                throw new FileNotFoundException( $"Could not find/acceess log file: {logFilePath}, AuditLogRootPath: {SynapseNodeService.Config.AuditLogRootPath}", ex );
            }
        }

        public void Start(CancellationToken token, Action<IPlanRuntimeContainer> callback)
        {
            token.Register( () => CancelPlanExecution() );
            Plan.Start( DynamicData, IsDryRun );

            SynapseNodeService.Logger.Info( $"SerializeResultPlan: {SynapseNodeService.Config.SerializeResultPlan}, {_logRootPath.FullName}\\{_ticks}_{Plan.Name}.result.yaml" );
            if( SynapseNodeService.Config.SerializeResultPlan )
                File.WriteAllText( $"{_logRootPath.FullName}\\{_ticks}_{Plan.Name}.result.yaml", Plan.ResultPlan.ToYaml() );

            callback?.Invoke( this );
        }

        private void CancelPlanExecution()
        {
            _wantsCancel = true;
        }

        public Plan Plan { get; }
        public bool IsDryRun { get; }
        public Dictionary<string, string> DynamicData { get; }
        public int PlanInstanceId { get; }


        private void Plan_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            if( _wantsCancel )
                e.Cancel = true;

            //todo: send a message home
        }

        private void Plan_LogMessage(object sender, LogMessageEventArgs e)
        {
            _log.Write( e.SerializeSimple() );
        }
    }
}