using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Synapse.Core.DataAccessLayer
{
    public struct ActionInstance
    {
        public const string Id = "Action_Instance_Id";
        public const string Name = "Action_Name";
        public const string PlanId = "Plan_Instance_Id";
        public const string PId = "Action_PId";
        public const string Status = "Action_Status";
        public const string StatusMsg = "Status_Message";
        public const string StatusSeq = "Status_Seq";
        public const string Dttm = "Modified_Dttm";
        public const string ParentId = "Parent_Id";

        public const string TableDef = @"
CREATE TABLE `Action_Instance` (
	`Action_Instance_Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	`Action_Name`	TEXT NOT NULL,
	`Plan_Instance_Id`	INTEGER NOT NULL,
	`Action_PId`	INTEGER NOT NULL,
	`Action_Status`	INTEGER NOT NULL,
	`Status_Message`	TEXT NOT NULL,
	`Status_Seq`	INTEGER NOT NULL,
	`Modified_Dttm`	INTEGER NOT NULL,
	`Parent_Id`	INTEGER,
	FOREIGN KEY(`Plan_Instance_Id`) REFERENCES `Plan_Instance`(`Plan_Instance_Id`)
);";
    }

    public partial class SynapseDal
    {
        public ActionItem GetActionById(int instanceId)
        {
            return null;
        }

        private void CreateAction(ref ActionItem action, long? parentId)
        {
            if( !action.HasResult )
                action.Result = new ExecuteResult();

            string parIdFld = parentId.HasValue ? $",{ActionInstance.ParentId}" : "";
            string parIdVal = parentId.HasValue ? $",{parentId.Value}" : "";

            string sql =
$@"insert into Action_Instance
(
    {ActionInstance.Name}
    ,{ActionInstance.PlanId}
    ,{ActionInstance.PId}
    ,{ActionInstance.Status}
    ,{ActionInstance.StatusMsg}
    ,{ActionInstance.StatusSeq}
    ,{ActionInstance.Dttm}
    {parIdFld}
)
values
(
    '{action.Name}'
    ,{action.PlanInstanceId}
    ,{action.Result.PId}
    ,{(int)action.Result.Status}
    ,'{action.Result.Status.ToString()}'
    ,{0}
    ,{GetEpoch()}
    {parIdVal}
)
";

            ExecuteNonQuery( sql );
            action.InstanceId = GetLastRowId().Value;
        }

        private void UpdateActionStatus(int instanceId, StatusType status, string message, int sequence)
        {
            string sql = $@"
update Action_Instance
set
    {ActionInstance.Status} = {(int)status}
    ,{ActionInstance.StatusMsg} = '{message}'
    ,{ActionInstance.StatusSeq} = {sequence}
    ,{ActionInstance.Dttm} = {GetEpoch()}
where
    {ActionInstance.Id} = {instanceId} and {ActionInstance.StatusSeq} < {sequence}
";

            ExecuteNonQuery( sql );
        }

        private void UpdateAction(ActionItem action)
        {
            string sql = $@"
update Action_Instance
set
    {ActionInstance.Name} = '{action.Name}'
    ,{ActionInstance.PlanId} = {action.PlanInstanceId}
    ,{ActionInstance.PId} = {action.Result.PId}
    ,{ActionInstance.Status} = {(int)action.Result.Status}
    ,{ActionInstance.StatusMsg} = '{"message"}'
    ,{ActionInstance.StatusSeq} = {0}
    ,{ActionInstance.Dttm} = {GetEpoch()}
    ,{ActionInstance.ParentId} = {0}
where
    {ActionInstance.Id} = {action.InstanceId}
";

            ExecuteNonQuery( sql );
        }

        public void DeleteAction(int instanceId)
        {
        }
    }
}