//#define sqlite  //uncomment this line to enable dal

#if !sqlite
namespace Synapse.Core.DataAccessLayer
{
    public partial class SynapseDal
    {
        public SynapseDal() { }

        static public void CreateDatabase() { }

        static public bool TestConnection(out System.Exception exception, out string message)
        {
            exception = null;
            message = null;
            return true;
        }
    }
}
#endif

#if sqlite

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Synapse.Core.DataAccessLayer
{
    public partial class SynapseDal
    {
        public static readonly string FileName = $"{Path.GetDirectoryName( typeof( SynapseDal ).Assembly.Location )}\\synapse.sqlite3";
        public static readonly string ConnectionString = $"Data Source={FileName};Version=3;";

        public SynapseDal() { }

        static public void CreateDatabase()
        {
            if( File.Exists( FileName ) && new FileInfo( FileName ).Length == 0 )
            {
                try { File.Delete( FileName ); }
                catch { };
            }

            if( !File.Exists( FileName ) )
            {
                try
                {
                    SQLiteConnection.CreateFile( FileName );

                    using( SQLiteConnection c = new SQLiteConnection( ConnectionString ) )
                    {
                        c.Open();
                        using( SQLiteCommand cmd = new SQLiteCommand( Plan.Fields.TableDef, c ) )
                        {
                            cmd.ExecuteNonQuery();
                        }
                        using( SQLiteCommand cmd = new SQLiteCommand( ActionItem.Fields.TableDef, c ) )
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch
                {
                    if( File.Exists( FileName ) && new FileInfo( FileName ).Length == 0 )
                        File.Delete( FileName );

                    throw;
                }
            }
        }

        static public bool TestConnection(out Exception exception, out string message)
        {
            bool ok = true;
            exception = null;

            SynapseDal dal = new SynapseDal();
            try
            {
                using( SQLiteConnection c = new SQLiteConnection( ConnectionString ) )
                {
                    c.Open();
                    message = $"Using: {c.FileName}";
                }
            }
            catch( Exception ex )
            {
                exception = ex;
                message = ex.Message;
                ok = false;
            }

            return ok;
        }

        internal void ExecuteNonQuery(string sql, SQLiteConnection connection = null)
        {
            if( connection == null )
            {
                using( SQLiteConnection c = new SQLiteConnection( ConnectionString ) )
                {
                    c.Open();
                    using( SQLiteCommand cmd = new SQLiteCommand( sql, c ) )
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using( SQLiteCommand cmd = new SQLiteCommand( sql, connection ) )
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal long? GetLastRowId(SQLiteConnection c)
        {
            long? id = 0;
            using( SQLiteCommand cmd = new SQLiteCommand( "select last_insert_rowid()", c ) )
            {
                id = (long?)cmd.ExecuteScalar();
            }

            return id;
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

#endif