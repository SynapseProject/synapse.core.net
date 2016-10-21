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
        SQLiteConnection _connection = new SQLiteConnection( "Data Source=synapse.db;Version=3;" );

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

        public void CreateDatabase()
        {
            string planInstance = "CREATE TABLE Plan_Instance ( `Plan_Instance_Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `Plan_Name` TEXT NOT NULL, `Request_Number` TEXT NOT NULL, `Plan_Status` INTEGER NOT NULL, `Status_Message` TEXT NOT NULL, `Modified_Dttm` INTEGER NOT NULL )";
            string actionInstance = "CREATE TABLE Action_Instance ( `Action_Instance_Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `Action_Name` TEXT NOT NULL, `Action_Detail` TEXT NOT NULL, `Plan_Instance_Id` INTEGER NOT NULL, `Log_Path` TEXT NOT NULL, `Action_Status` INTEGER NOT NULL, `Status_Message` TEXT NOT NULL, `Status_Seq` INTEGER NOT NULL, `Modified_Dttm` INTEGER NOT NULL, `Parent_Id` INTEGER, FOREIGN KEY(Plan_Instance_Id) REFERENCES Plan_Instance(Plan_Instance_Id) )";
            string seq = "CREATE TABLE sqlite_sequence(name,seq)";

            SQLiteConnection.CreateFile( "synapse.db" );
            OpenConnection();
            ExecuteNonQuery( planInstance );
            ExecuteNonQuery( actionInstance );
            ExecuteNonQuery( seq );
            CloseConnection();
        }

        public void ExecuteNonQuery(string sql)
        {
            new SQLiteCommand( sql, _connection ).ExecuteNonQuery();
        }
    }
}