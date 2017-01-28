using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;

using Synapse.Core.Utilities;

namespace Synapse.Core.Runtime
{
    [ServiceContract( Namespace = "http://Synapse.Services.Node" )]
    public interface ISynapseNodeServer
    {
        #region smoke tests
        [OperationContract]
        [WebGet( UriTemplate = "/hello" ), Description( "Say, \"hi!\"" )]
        string Hello();

        [OperationContract]
        [WebGet( UriTemplate = "/hello/whoami/" ), Description( "Get security/connection information: /hello/?whoami" )]
        string WhoAmI();

        //[OperationContract]
        //[WebGet( UriTemplate = "/hello/?whoami" ), Description( "Get security/connection information: /hello/?whoami" )]
        //WhoAmIRecord WhoAmI();
        #endregion

        #region SynapseNodeServer
        [OperationContract]
        [WebInvoke( Method = HttpMethod.Post, UriTemplate = "/execute/sync/{planInstanceId}/?action=start&dryRun={dryRun}" )]
        ExecuteResult StartPlan(string planInstanceId, bool dryRun, Plan plan);

        [OperationContract]
        [WebInvoke( Method = HttpMethod.Post, UriTemplate = "/execute/{planInstanceId}/?action=start&dryRun={dryRun}" )]
        void StartPlanAsync(string planInstanceId, bool dryRun, Plan plan);

        [OperationContract]
        [WebGet( UriTemplate = "/execute/{planInstanceId}/?action=cancel" ), Description( "[?action=cancel] Cancels Plan execution at first opportunity." )]
        void CancelPlan(string planInstanceId);


        [OperationContract]
        [WebGet( UriTemplate = "/drainstop/?action=stop&shutdown={shutdown}" ), Description( "[?action=stop&shutdown=true/false] Prevents the TaskScheduler from accepting new work; allows existing threads to complete; optionally shuts down the service then the queue is fully drained." )]
        void Drainstop(bool shutdown);

        [OperationContract]
        [WebGet( UriTemplate = "/drainstop/?action=unstop" ), Description( "[?action=unstop] Returns the TaskScheduler to a normal state." )]
        void Undrainstop();

        [OperationContract]
        [WebGet( UriTemplate = "/drainstop/?action=status" ), Description( "[?action=status] Returns the TaskScheduler Drainstop 'Complete' status." )]
        bool GetIsDrainstopComplete();


        [OperationContract]
        [WebGet( UriTemplate = "/queue/?action=depth" ), Description( "[?action=depth] Returns the number of executing plans." )]
        int GetCurrentQueueDepth();

        [OperationContract]
        [WebGet( UriTemplate = "/queue/?action=list" ), Description( "[?action=list] Returns the list of executing plans." )]
        List<string> GetCurrentQueueItems();
        #endregion
    }
}