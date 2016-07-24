using Synapse.Core;

namespace Synapse.Core.Runtime
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

		public HandlerResult StartPlan(string planInstanceId, bool dryRun, Plan plan)
		{
			return plan.Start( null, dryRun );
		}

		public void StartPlanAsync(string planInstanceId, bool dryRun, Plan plan)
		{
			_scheduler.StartPlan( null, dryRun, plan );
		}

		#endregion
	}
}