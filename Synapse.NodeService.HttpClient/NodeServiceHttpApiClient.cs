using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Synapse.Core;
using Synapse.Common.WebApi;

namespace Synapse.Services
{
    public class NodeServiceHttpApiClient : HttpApiClientBase
    {
        string _rootPath = "/synapse/node";

        public NodeServiceHttpApiClient(string baseUrl, string messageFormatType = "application/json") : base( baseUrl, messageFormatType )
        {
        }


        public ExecuteResult StartPlan(int planInstanceId, bool dryRun, Plan plan)
        {
            return StartPlanAsync( planInstanceId, dryRun, plan ).Result;
        }

        public async Task<ExecuteResult> StartPlanAsync(int planInstanceId, bool dryRun, Plan plan)
        {
            string requestUri = $"{_rootPath}/execute/{planInstanceId}/?action=start&dryRun={dryRun}";
            return await PostAsync<Plan, ExecuteResult>( plan, requestUri );
        }

        public void CancelPlan(long planInstanceId)
        {
            CancelPlanAsync( planInstanceId ).Wait();
        }

        public async Task CancelPlanAsync(long planInstanceId)
        {
            string requestUri = $"{_rootPath}/execute/{planInstanceId}/?action=cancel";
            await GetAsync( requestUri );
        }


        public void Drainstop()
        {
            DrainstopAsync().Wait();
        }

        public async Task DrainstopAsync()
        {
            string requestUri = $"{_rootPath}/drainstop/?action=stop";
            await GetAsync( requestUri );
        }

        public void Undrainstop()
        {
            UndrainstopAsync().Wait();
        }

        public async Task UndrainstopAsync()
        {
            string requestUri = $"{_rootPath}/drainstop/?action=unstop";
            await GetAsync( requestUri );
        }
    }
}