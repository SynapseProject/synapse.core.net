using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.Core.Runtime
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
        [WebInvoke( Method = HttpMethod.Post, UriTemplate = "/execute/{planInstanceId}/?action=start&dryRun={dryRun}" )]
        HandlerResult StartPlan(string planInstanceId, bool dryRun, Plan plan);
        #endregion
    }
}