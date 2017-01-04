using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using Synapse.Core.DataAccessLayer;
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
            this.ServiceName = "Synapse.Service";
            InitializeComponent();
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            if( Environment.UserInteractive && args.Length > 0 )
            {
                string arg0 = args[0].ToLower();
                if( arg0 == "/install" || arg0 == "/i" )
                    InstallUtility.InstallService( install: true );
                else if( arg0 == "/uninstall" || arg0 == "/u" )
                    InstallUtility.InstallService( install: false );

                Environment.Exit( 0 );
            }


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
            try
            {
                _log.Write( ServiceStatus.Starting );

                EnsureDatabase();

                if( _serviceHost != null )
                    _serviceHost.Close();

                _serviceHost = new ServiceHost( typeof( SynapseServer ) );
                _serviceHost.Open();

                _log.Write( ServiceStatus.Running );
            }
            catch(Exception ex)
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

        protected override void OnStop()
        {
            _log.Write( ServiceStatus.Stopping );
            if( _serviceHost != null )
                _serviceHost.Close();
            _log.Write( ServiceStatus.Stopped );
        }


        #region ensure database exists
        void EnsureDatabase()
        {
            _log.Write( "EnsureDatabase: Checking file exists and connection is valid." );
            SynapseDal.CreateDatabase();
            Exception testResult = null;
            string message = string.Empty;
            if( !SynapseDal.TestConnection( out testResult, out message ) )
                throw testResult;
            _log.Write( $"EnsureDatabase: Success. {message}" );
        }
        #endregion

        #region exception handling
        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string source = "SynapseService";
            string log = "Application";

            string msg = ((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).InnerException.Message;

            try
            {
                if( !EventLog.SourceExists( source ) )
                    EventLog.CreateEventSource( source, log );

                EventLog.WriteEntry( source, msg, EventLogEntryType.Error );
            }
            catch { }

            try
            {
                LogManager _log = new LogManager();
                _log.Write( LogLevel.Fatal, msg );
            }
            catch
            {
                System.IO.File.AppendAllText( @"C:\Temp\error.txt", ((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).InnerException.Message );
            }
        }

        void WriteEventLog(string msg, EventLogEntryType entryType = EventLogEntryType.Error)
        {
            string source = "SynapseService";
            string log = "Application";

            try
            {
                if( !EventLog.SourceExists( source ) )
                    EventLog.CreateEventSource( source, log );

                EventLog.WriteEntry( source, msg, entryType );
            }
            catch { }
        }
        #endregion
    }
}