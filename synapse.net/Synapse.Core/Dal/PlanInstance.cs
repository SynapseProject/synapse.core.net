using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Synapse.Core.DataAccessLayer
{
    public partial class SynapseDal
    {
        public struct PlanInstance
        {
            public const string Id = "Plan_Instance_Id";
            public const string Name = "Plan_Name";
            public const string ReqNum = "Request_Number";
            public const string LogPath = "Log_Path";
            public const string PId = "Plan_PId";
            public const string Status = "Plan_Status";
            public const string StatusMsg = "Status_Message";
            public const string Dttm = "Modified_Dttm";

            public const string TableDef = @"
CREATE TABLE `Plan_Instance` (
	`Plan_Instance_Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`Plan_Name`	TEXT NOT NULL,
	`Request_Number`	TEXT NOT NULL,
	`Log_Path`	TEXT NOT NULL,
	`Plan_PId`	INTEGER NOT NULL,
	`Plan_Status`	INTEGER NOT NULL,
	`Status_Message`	TEXT NOT NULL,
	`Modified_Dttm`	INTEGER NOT NULL
);";
        }

        public Plan GetPlanById(int instanceId)
        {
            return null;
        }

        private void CreatePlan(ref Plan plan)
        {
            if( !plan.HasResult )
                plan.Result = new ExecuteResult();

            string sql =
$@"insert into Plan_Instance
(
    {PlanInstance.Name}
    ,{PlanInstance.ReqNum}
    ,{PlanInstance.LogPath}
    ,{PlanInstance.PId}
    ,{PlanInstance.Status}
    ,{PlanInstance.StatusMsg}
    ,{PlanInstance.Dttm}
)
values
(
    '{plan.Name}'
    ,'{"plan.RequestNumber"}'
    ,'{"plan.LogPath"}'
    ,{plan.Result.PId}
    ,{(int)plan.Result.Status}
    ,'{plan.Result.Status.ToString()}'
    ,{GetEpoch()}
)
";

            ExecuteNonQuery( sql );
            plan.InstanceId = GetLastRowId().Value;
        }

        private void UpdatePlanStatus(int instanceId, StatusType status, string message)
        {
            string sql = $@"
update Action_Instance
set
    {PlanInstance.Status} = {(int)status}
    ,{PlanInstance.StatusMsg} = '{message}'
    ,{PlanInstance.Dttm} = {GetEpoch()}
where
    {PlanInstance.Id} = {instanceId}
";

            ExecuteNonQuery( sql );
        }

        public void DeletePlan(int instanceId)
        {
        }
    }
}