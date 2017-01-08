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
            {
                _scheduler = new PlanScheduler( SynapseService.Config.MaxServerThreads );
                SynapseService.Logger.Info( $"Initialized PlanScheduler, MaxThreads: {SynapseService.Config.MaxServerThreads}" );
            }
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

            SynapseService.Logger.Info( $"StartPlanAsync: InstanceId: {planInstId}, Name: {plan.Name}" );

            PlanRuntimePod p = new PlanRuntimePod( plan, dryRun, null, planInstId );
            _scheduler.StartPlan( p );  //_scheduler.StartPlan( null, dryRun, plan );
        }

        public void CancelPlan(string planInstanceId)
        {
            int planInstId = int.Parse( planInstanceId );
            bool found = _scheduler.CancelPlan( planInstId );
            string foundMsg = found ?
                "Found executing Plan and signaled Cancel request." :
                "Could not find executing Plan; Plan may have already completed execution.";
            SynapseService.Logger.Info( $"CancelPlan {planInstId}: {foundMsg}" );
        }

        public void Drainstop()
        {
            SynapseService.Logger.Info( $"Drainstop starting, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
            _scheduler.Drainstop();
            SynapseService.Logger.Info( $"Drainstop complete, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
        }

        public void Undrainstop()
        {
            SynapseService.Logger.Info( $"Undrainstop starting, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
            _scheduler.Undrainstop();
            SynapseService.Logger.Info( $"Undrainstop complete, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
        }

        public bool GetIsDrainstopComplete() { return _scheduler.IsDrainstopComplete; }
        #endregion
    }
}