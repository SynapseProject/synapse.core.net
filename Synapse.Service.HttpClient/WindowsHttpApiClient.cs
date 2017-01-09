﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Synapse.Core;
using Synapse.Common.WebApi;

namespace Synapse.Service.Windows
{
    public class HttpApiClient : HttpApiClientBase
    {
        public HttpApiClient(string baseUrl, string messageFormatType = "application/json") : base( baseUrl, messageFormatType )
        {
        }


        public ExecuteResult StartPlan(int planInstanceId, bool dryRun, Plan plan)
        {
            return StartPlanAsync( planInstanceId, dryRun, plan ).Result;
        }

        public async Task<ExecuteResult> StartPlanAsync(int planInstanceId, bool dryRun, Plan plan)
        {
            string requestUri = $"/syn/server/execute/{planInstanceId}/?action=start&dryRun={dryRun}";
            return await PostAsync<Plan, ExecuteResult>( plan, requestUri );
        }

        public void CancelPlan(int planInstanceId)
        {
            CancelPlanAsync( planInstanceId ).Wait();
        }

        public async Task CancelPlanAsync(int planInstanceId)
        {
            string requestUri = $"/execute/{planInstanceId}/?action=cancel";
            await GetAsync( requestUri );
        }


        public void Drainstop()
        {
            DrainstopAsync().Wait();
        }

        public async Task DrainstopAsync()
        {
            string requestUri = $"/drainstop/?action=stop";
            await GetAsync( requestUri );
        }

        public void Undrainstop()
        {
            UndrainstopAsync().Wait();
        }

        public async Task UndrainstopAsync()
        {
            string requestUri = $"/drainstop/?action=unstop";
            await GetAsync( requestUri );
        }
    }
}