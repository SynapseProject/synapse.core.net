using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Synapse.Core.DataAccessLayer;

namespace Synapse.Core
{
    public partial class ActionItem
    {
        SynapseDal _dal = new SynapseDal();

        internal struct Fields
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

            public const string TableName = "Action_Instance";
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

        internal ActionItem GetInstance()
        {
            return null;
        }

        /// <summary>
        /// Creates an instance of this action in the sqlite db for tracking status.
        /// </summary>
        /// <param name="parentContext">The parent of the action; if the parent is the Plan, ParentId = null.</param>
        internal void CreateInstance(IActionContainer parentContext, long planInstanceId)
        {
            PlanInstanceId = planInstanceId;
            long? parentId = null;
            if( parentContext is ActionItem )
                parentId = ((ActionItem)parentContext).InstanceId;
            CreateInstance( parentId );
        }

        internal void CreateInstance(long? parentId)
        {
            if( !HasResult )
                Result = new ExecuteResult();

            string parIdFld = parentId.HasValue ? $",{Fields.ParentId}" : "";
            string parIdVal = parentId.HasValue ? $",{parentId.Value}" : "";

            string sql =
$@"insert into {Fields.TableName}
(
    {Fields.Name}
    ,{Fields.PlanId}
    ,{Fields.PId}
    ,{Fields.Status}
    ,{Fields.StatusMsg}
    ,{Fields.StatusSeq}
    ,{Fields.Dttm}
    {parIdFld}
)
values
(
    '{Name}'
    ,{PlanInstanceId}
    ,{Result.PId}
    ,{(int)Result.Status}
    ,'{Result.Status.ToString()}'
    ,{-100}
    ,{_dal.GetEpoch()}
    {parIdVal}
)
";

            _dal.ExecuteNonQuery( sql );
            InstanceId = _dal.GetLastRowId().Value;
        }

        internal void UpdateInstanceStatus(StatusType status, string message, int sequence, int? pid = null)
        {
            string pidsql = pid.HasValue ? $",{Fields.PId} = {pid.Value}" : string.Empty;
            string sql = $@"
update {Fields.TableName}
set
    {Fields.Status} = {(int)status}
    ,{Fields.StatusMsg} = '{message.Replace( "'", "`" )}'
    ,{Fields.StatusSeq} = {sequence}
    ,{Fields.Dttm} = {_dal.GetEpoch()}
    {pidsql}
where
    {Fields.Id} = {InstanceId} and {Fields.StatusSeq} < {sequence}
";

            _dal.ExecuteNonQuery( sql, CommandBehavior.CloseConnection );
        }

        private void UpdateInstance(ActionItem action)
        {
            string sql = $@"
update {Fields.TableName}
set
    {Fields.Name} = '{Name}'
    ,{Fields.PlanId} = {PlanInstanceId}
    ,{Fields.PId} = {Result.PId}
    ,{Fields.Status} = {(int)Result.Status}
    ,{Fields.StatusMsg} = '{"message"}'
    ,{Fields.StatusSeq} = {0}
    ,{Fields.Dttm} = {_dal.GetEpoch()}
    ,{Fields.ParentId} = {0}
where
    {Fields.Id} = {InstanceId}
";

            _dal.ExecuteNonQuery( sql, CommandBehavior.CloseConnection );
        }

        public void DeleteInstance()
        {
        }
    }
}