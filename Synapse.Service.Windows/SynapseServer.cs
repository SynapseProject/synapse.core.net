using Synapse.Core;
using Synapse.Core.Runtime;

namespace Synapse.Service.Windows
{
    public class SynapseServer : ISynapseServer
    {
        PlanScheduler _scheduler = null;

        public SynapseServer()
        {
            _scheduler = new PlanScheduler( 10 );
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
            PlanRuntimePod p = new PlanRuntimePod( plan, dryRun, null );
            p.InitializeLogger();
            _scheduler.StartPlan( p );

            //_scheduler.StartPlan( null, dryRun, plan );
        }

        #endregion
    }
}