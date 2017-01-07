using System;
using Synapse.Core;
using Synapse.Core.Runtime;

namespace Synapse.Service.Windows
{
    public class SynapseServer : ISynapseServer
    {
        static PlanScheduler _scheduler = null;

        public SynapseServer()
        {
            if( _scheduler == null )
                _scheduler = new PlanScheduler( SynapseService.Config.MaxServerThreads );
        }

        #region ISynapseServer Members

        public string Hello()
        {
            return "Hello from SynapseServer, World!";
        }

        public string WhosHere()
        {
            return "WhosHere from SynapseServer, World!";
        }

        public ExecuteResult StartPlan(string planInstanceId, bool dryRun, Plan plan)
        {
            return plan.Start( null, dryRun );
        }

        public void StartPlanAsync(string planInstanceId, bool dryRun, Plan plan)
        {
            int planInstId = int.Parse( planInstanceId );

            PlanRuntimePod p = new PlanRuntimePod( plan, dryRun, null, planInstId );
            _scheduler.StartPlan( p );

            //_scheduler.StartPlan( null, dryRun, plan );
        }

        public void CancelPlan(string planInstanceId)
        {
            int planInstId = int.Parse( planInstanceId );
            _scheduler.CancelPlan( planInstId );
        }

        public void Drainstop()
        {
            _scheduler.Drainstop();
        }

        public void Undrainstop()
        {
            _scheduler.Undrainstop();
        }

        public bool GetIsDrainstopComplete() { return _scheduler.IsDrainstopComplete; }
        #endregion
    }
}