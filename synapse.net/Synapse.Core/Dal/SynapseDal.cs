﻿using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Synapse.Core.DataAccessLayer
{
    public partial class SynapseDal
    {
        const string _fileName = "synapse.sqlite3";
        SQLiteConnection _connection = null;

        public SynapseDal()
        {
            if( File.Exists( _fileName ) )
                _connection = new SQLiteConnection( $"Data Source={_fileName};Version=3;" );
        }

        static public void CreateDatabase()
        {
            if( !File.Exists( _fileName ) )
            {
                SQLiteConnection.CreateFile( _fileName );

                SynapseDal dal = new SynapseDal();
                dal.OpenConnection();
                dal.ExecuteNonQuery( Plan.Fields.TableDef );
                dal.ExecuteNonQuery( ActionItem.Fields.TableDef );
                dal.CloseConnection();
            }
        }

        internal void OpenConnection()
        {
            if( _connection == null )
                _connection = new SQLiteConnection( $"Data Source={_fileName};Version=3;" );

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
            OpenConnection();
            new SQLiteCommand( sql, _connection ).ExecuteNonQuery( commandBehavior );
        }

        internal long? GetLastRowId()
        {
            OpenConnection();
            return (long?)(new SQLiteCommand( "select last_insert_rowid()", _connection ).ExecuteScalar());
        }

        internal int GetEpoch()
        {
            return (int)(DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc )).TotalSeconds;
        }

        internal static void UpdateActionInstance(long instanceId, StatusType status, string message, int sequence, int? pid = null)
        {
            (new ActionItem() { InstanceId = instanceId }).UpdateInstanceStatus( status, message, sequence, pid );
        }
    }
}