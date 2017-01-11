using System;
using Synapse.Core;
using Synapse.Core.Runtime;
using System.Collections.Generic;

namespace Synapse.Service.Windows
{
    public class SynapseServer : ISynapseServer
    {
        static PlanScheduler _scheduler = null;

        public SynapseServer()
        {
            InitPlanScheduler();
        }

        public static void InitPlanScheduler()
        {
            if( _scheduler == null )
            {
                _scheduler = new PlanScheduler( SynapseService.Config.MaxServerThreads );
                _scheduler.PlanCompleted += Scheduler_PlanCompleted;
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

        private static void Scheduler_PlanCompleted(object sender, PlanCompletedEventArgs e)
        {
            SynapseService.Logger.Info( $"Plan Completed: InstanceId: {e.PlanContainer.PlanInstanceId}, Name: {e.PlanContainer.Plan.Name}" );  //, At: {e.TimeCompleted}
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

        public int GetCurrentQueueDepth() { return _scheduler.CurrentQueueDepth; }

        public List<string> GetCurrentQueueItems() { return _scheduler.CurrentQueue; }
        #endregion
    }
}