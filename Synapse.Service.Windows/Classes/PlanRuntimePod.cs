using System;
using System.Collections.Generic;

using Synapse.Common;
using Synapse.Core;
using Synapse.Core.Runtime;

namespace Synapse.Service.Windows
{
    public class PlanRuntimePod : IPlanRuntimeContainer
    {
        LogManager _log = new LogManager();

        public PlanRuntimePod(Plan plan, bool isDryRun = false, Dictionary<string, string> dynamicData = null)
        {
            Plan = plan;
            IsDryRun = isDryRun;
            DynamicData = dynamicData;

            InitializeLogger();

            Plan.Progress += Plan_Progress;
            Plan.LogMessage += Plan_LogMessage;
        }

        public void InitializeLogger()
        {
            string logFileName = $"{Plan.Name}_{DateTime.Now.Ticks}";
            string logFilePath = $"{System.IO.Path.GetDirectoryName( typeof( PlanRuntimePod ).Assembly.Location )}\\{logFileName}.log";
            _log.InitDynamicFileAppender( logFileName, logFileName, logFilePath, "%d{ISO8601}|%-5p|(%t)|%m%n", "all" );
        }

        public void Start()
        {
            Plan.Start( DynamicData, IsDryRun );
        }

        public Plan Plan { get; }
        public bool IsDryRun { get; }
        public Dictionary<string, string> DynamicData { get; }


        private void Plan_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            //todo: send a message home
        }

        private void Plan_LogMessage(object sender, LogMessageEventArgs e)
        {
            _log.Write( e.SerializeSimple() );
        }
    }
}