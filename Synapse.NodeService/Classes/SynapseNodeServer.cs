using System;
using Synapse.Core;
using Synapse.Core.Runtime;
using System.Collections.Generic;

namespace Synapse.Services
{
    public class SynapseNodeServer : ISynapseNodeServer
    {
        static PlanScheduler _scheduler = null;

        public SynapseNodeServer()
        {
            InitPlanScheduler();
        }

        public static void InitPlanScheduler()
        {
            if( _scheduler == null )
            {
                _scheduler = new PlanScheduler( SynapseNodeService.Config.MaxServerThreads );
                _scheduler.PlanCompleted += Scheduler_PlanCompleted;
                SynapseNodeService.Logger.Info( $"Initialized PlanScheduler, MaxThreads: {SynapseNodeService.Config.MaxServerThreads}" );
            }
        }

        #region ISynapseServer Members

        public string Hello()
        {
            return "Hello from SynapseNodeServer, World!";
        }

        public string WhosHere()
        {
            return "WhosHere from SynapseNodeServer, World!";
        }

        public ExecuteResult StartPlan(string planInstanceId, bool dryRun, Plan plan)
        {
            return plan.Start( null, dryRun );
        }

        public void StartPlanAsync(string planInstanceId, bool dryRun, Plan plan)
        {
            int planInstId = int.Parse( planInstanceId );

            SynapseNodeService.Logger.Info( $"StartPlanAsync: InstanceId: {planInstId}, Name: {plan.Name}" );

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
            SynapseNodeService.Logger.Info( $"CancelPlan {planInstId}: {foundMsg}" );
        }

        private static void Scheduler_PlanCompleted(object sender, PlanCompletedEventArgs e)
        {
            SynapseNodeService.Logger.Info( $"Plan Completed: InstanceId: {e.PlanContainer.PlanInstanceId}, Name: {e.PlanContainer.Plan.Name}" );  //, At: {e.TimeCompleted}
        }

        public void Drainstop()
        {
            SynapseNodeService.Logger.Info( $"Drainstop starting, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
            _scheduler.Drainstop();
            SynapseNodeService.Logger.Info( $"Drainstop complete, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
        }

        public void Undrainstop()
        {
            SynapseNodeService.Logger.Info( $"Undrainstop starting, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
            _scheduler.Undrainstop();
            SynapseNodeService.Logger.Info( $"Undrainstop complete, CurrentQueueDepth: {_scheduler.CurrentQueueDepth}" );
        }

        public bool GetIsDrainstopComplete() { return _scheduler.IsDrainstopComplete; }

        public int GetCurrentQueueDepth() { return _scheduler.CurrentQueueDepth; }

        public List<string> GetCurrentQueueItems() { return _scheduler.CurrentQueue; }
        #endregion
    }
}