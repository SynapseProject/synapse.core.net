using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core;

namespace Synapse.Server
{
	public class SynapseServer : ISynapseServer
	{
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

		#endregion
	}
}