#if sqlite

using System;
using System.Collections.Generic;

using System.Data.SQLite;

using Synapse.Core.DataAccessLayer;

namespace Synapse.Core
{
    public partial class Plan
    {
        SynapseDal _dal = new SynapseDal();
        long _instanceId = 0;

        internal struct Fields
        {
            public const string Id = "Plan_Instance_Id";
            public const string Name = "Plan_Name";
            public const string ReqNum = "Request_Number";
            public const string LogPath = "Log_Path";
            public const string PId = "Plan_PId";
            public const string Status = "Plan_Status";
            public const string StatusMsg = "Status_Message";
            public const string Dttm = "Modified_Dttm";

            public const string TableName = "Plan_Instance";
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

        internal Plan GetInstance()
        {
            return null;
        }

        internal void CreateInstance()
        {
            CreateInstanceInternal();
        }
        [Obsolete( "remove", true )]
        void RecurseActions(List<ActionItem> actions, long? parentId, long planId)
        {
            foreach( ActionItem action in actions )
            {
                action.PlanInstanceId = planId;
                action.CreateInstance( parentId );

                if( action.HasActionGroup )
                {
                    action.ActionGroup.PlanInstanceId = planId;
                    action.ActionGroup.CreateInstance( parentId );

                    if( action.ActionGroup.HasActions )
                        RecurseActions( action.ActionGroup.Actions, action.ActionGroup.InstanceId, planId );
                }

                if( action.HasActions )
                    RecurseActions( action.Actions, action.InstanceId, planId );
            }
        }

        void CreateInstanceInternal()
        {
            if( !HasResult )
                Result = new ExecuteResult();

            string sql =
$@"insert into {Fields.TableName}
(
    {Fields.Name}
    ,{Fields.ReqNum}
    ,{Fields.LogPath}
    ,{Fields.PId}
    ,{Fields.Status}
    ,{Fields.StatusMsg}
    ,{Fields.Dttm}
)
values
(
    '{Name}'
    ,'{StartInfo.RequestNumber}'
    ,'{"plan.LogPath"}'
    ,{Result.PId}
    ,{(int)Result.Status}
    ,'{Result.Status.ToString()}'
    ,{_dal.GetEpoch()}
)
";

            using( SQLiteConnection c = new SQLiteConnection( SynapseDal.ConnectionString ) )
            {
                c.Open();

                _dal.ExecuteNonQuery( sql, c );
                _instanceId = _dal.GetLastRowId( c ).Value;
            }

            if( InstanceId == 0 )
                InstanceId = _instanceId;
        }

        internal void UpdateInstanceStatus(StatusType status, string message)
        {
            string sql = $@"
update {Fields.TableName}
set
    {Fields.Status} = {(int)status}
    ,{Fields.StatusMsg} = '{message}'
    ,{Fields.Dttm} = {_dal.GetEpoch()}
where
    {Fields.Id} = {_instanceId}
";

            _dal.ExecuteNonQuery( sql );
        }

        internal void DeleteInstance(int instanceId)
        {
        }
    }
}

#endif