using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Synapse.Core.DataAccessLayer
{
    public partial class SynapseDal
    {
        const string _fileName = "synapse.sqlite3";
        SQLiteConnection _connection = new SQLiteConnection( $"Data Source={_fileName};Version=3;" );

        public void CreateDatabase()
        {
            if( !File.Exists( _fileName ) )
            {
                SQLiteConnection.CreateFile( _fileName );
                OpenConnection();
                ExecuteNonQuery( PlanInstance.TableDef );
                ExecuteNonQuery( ActionInstance.TableDef );
                CloseConnection();
            }
        }

        public void CreatePlanInstance(ref Plan plan)
        {
            OpenConnection();
            CreatePlan( ref plan );
            RecurseActions( plan.Actions, null, plan.InstanceId );
            CloseConnection();
        }
        void RecurseActions(List<ActionItem> actions, long? parentId, long planId)
        {
            foreach( ActionItem action in actions )
            {
                ActionItem a = action;
                a.PlanInstanceId = planId;
                CreateAction( ref a, parentId );

                if( a.HasActionGroup )
                {
                    ActionItem ag = a.ActionGroup;
                    ag.PlanInstanceId = planId;
                    CreateAction( ref ag, parentId );

                    if( ag.HasActions )
                        RecurseActions( ag.Actions, ag.InstanceId, planId );
                }

                if( a.HasActions )
                    RecurseActions( a.Actions, a.InstanceId, planId );
            }
        }

        #region utility
        public void OpenConnection()
        {
            if( _connection.State != ConnectionState.Open )
                _connection.Open();
        }

        public void CloseConnection()
        {
            if( _connection.State != ConnectionState.Closed )
                _connection.Close();
        }

        public void ExecuteNonQuery(string sql)
        {
            new SQLiteCommand( sql, _connection ).ExecuteNonQuery();
        }

        public long? GetLastRowId()
        {
            return (long?)(new SQLiteCommand( "select last_insert_rowid()", _connection ).ExecuteScalar());
        }

        int GetEpoch()
        {
            return (int)(DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc )).TotalSeconds;
        }
        #endregion
    }
}