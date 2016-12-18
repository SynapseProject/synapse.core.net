using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;

using Synapse.Core.Runtime;
using Synapse.Service.Common;

namespace Synapse.Service.Windows
{
    public partial class SynapseService : ServiceBase
    {
        LogManager _log = new LogManager();
        ServiceHost _serviceHost = null;

        public SynapseService()
        {
            InitializeComponent();
        }

        static void Main()
        {
#if DEBUG
            SynapseService s = new SynapseService();
            s.OnDebugMode_Start();
            System.Threading.Thread.Sleep( System.Threading.Timeout.Infinite );
            s.OnDebugMode_Stop();
#else
			ServiceBase.Run( new SynapseService() );
#endif
        }

        public void OnDebugMode_Start()
        {
            OnStart( null );
        }
        public void OnDebugMode_Stop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            _log.Write( ServiceStatus.Starting );

            EnsureDatabaseExists();

            if( _serviceHost != null )
                _serviceHost.Close();

            _serviceHost = new ServiceHost( typeof( SynapseServer ) );
            _serviceHost.Open();

            _log.Write( ServiceStatus.Running );
        }

        protected override void OnStop()
        {
            _log.Write( ServiceStatus.Stopping );
            if( _serviceHost != null )
                _serviceHost.Close();
            _log.Write( ServiceStatus.Stopped );
        }


        #region ensure database exists
        void EnsureDatabaseExists()
        {
            try
            {
                Synapse.Core.DataAccessLayer.SynapseDal.CreateDatabase();
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
                if( ex.HResult == -2146233052 )
                    msg += "  Ensure the x86/x64 Sqlite folders are included with the distribution.";

                _log.Write( LogLevel.Fatal, msg );
                WriteEventLog( msg );

                this.Stop();
                Environment.Exit( 99 );
            }
        }
        #endregion

        void WriteEventLog(string msg, EventLogEntryType entryType = EventLogEntryType.Error)
        {
            string source = "Synapse";
            string log = "Application";

            try
            {
                if( !EventLog.SourceExists( source ) )
                    EventLog.CreateEventSource( source, log );

                EventLog.WriteEntry( source, msg, entryType );
            }
            catch { }
        }
    }
}