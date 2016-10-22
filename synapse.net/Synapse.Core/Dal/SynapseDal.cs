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

        static public void CreateDatabase()
        {
            SynapseDal dal = new SynapseDal();
            if( !File.Exists( _fileName ) )
            {
                SQLiteConnection.CreateFile( _fileName );
                dal.OpenConnection();
                dal.ExecuteNonQuery( Plan.Fields.TableDef );
                dal.ExecuteNonQuery( ActionItem.Fields.TableDef );
                dal.CloseConnection();
            }
        }

        internal void OpenConnection()
        {
            if( _connection.State != ConnectionState.Open )
                _connection.Open();
        }

        internal void CloseConnection()
        {
            if( _connection.State != ConnectionState.Closed )
                _connection.Close();
        }

        internal void ExecuteNonQuery(string sql, CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            new SQLiteCommand( sql, _connection ).ExecuteNonQuery( commandBehavior );
        }

        internal long? GetLastRowId()
        {
            return (long?)(new SQLiteCommand( "select last_insert_rowid()", _connection ).ExecuteScalar());
        }

        internal int GetEpoch()
        {
            return (int)(DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc )).TotalSeconds;
        }
    }
}