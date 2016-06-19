using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;

using Synapse.Core;

namespace Synapse.Server
{
	[ServiceContract( Namespace = "http://Synapse.Server" )]
	public interface ISynapseServer
	{
		#region smoke tests
		[OperationContract]
		[WebGet( UriTemplate = "/hello" ), Description( "Say, \"hi!\"" )]
		string Hello();

		[OperationContract]
		[WebGet( UriTemplate = "/hello/whoami/" ), Description( "Get security/connection information: /hello/?whoshere" )]
		string WhosHere();

		//[OperationContract]
		//[WebGet( UriTemplate = "/hello/?whoami" ), Description( "Get security/connection information: /hello/?whoami" )]
		//WhoAmIRecord WhoAmI();
		#endregion

		#region
		[OperationContract]
		[WebGet( UriTemplate = "/hello" ), Description( "Say, \"hi!\"" )]
		HandlerResult StartPlan(Plan plan, bool dryRun);
		#endregion
	}
}