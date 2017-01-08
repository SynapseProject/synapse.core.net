using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Threading;

using Synapse.Core.DataAccessLayer;
using Synapse.Service.Common;
using log4net;

namespace Synapse.Service.Windows
{
    public partial class SynapseService : ServiceBase
    {
        public static ILog Logger = LogManager.GetLogger( "SynapseServer" );
        public static SynapseServiceConfig Config = null;

        ServiceHost _serviceHost = null;


        public SynapseService()
        {
            this.ServiceName = "Synapse.Service";
            InitializeComponent();
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            InstallService( args );

            Config = SynapseServiceConfig.Deserialze();

#if DEBUG
            SynapseService s = new SynapseService();
            s.OnStart( null );
            Thread.Sleep( Timeout.Infinite );
            s.OnStop();
#else
			ServiceBase.Run( new SynapseService() );
#endif
        }

        /// <summary>
        /// Install/Uninstall the service.
        /// Only works for Release build, as Debug will timeout on service start anyway (Thread.Sleep( Timeout.Infinite );).
        /// </summary>
        /// <param name="args"></param>
        [Conditional( "RELEASE" )]
        static void InstallService(string[] args)
        {
            if( Environment.UserInteractive )
                if( args.Length > 0 )
                {
                    bool ok = false;
                    string message = string.Empty;

                    string arg0 = args[0].ToLower();
                    if( arg0 == "/install" || arg0 == "/i" )
                        ok = InstallUtility.InstallService( install: true, message: out message );
                    else if( arg0 == "/uninstall" || arg0 == "/u" )
                        ok = InstallUtility.InstallService( install: false, message: out message );

                    if( !ok )
                        WriteHelpAndExit( message );
                    else
                        Environment.Exit( 0 );
                }
                else
                {
                    WriteHelpAndExit();
                }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Info( ServiceStatus.Starting );

                EnsureDatabase();

                if( _serviceHost != null )
                    _serviceHost.Close();

                _serviceHost = new ServiceHost( typeof( SynapseServer ) );
                _serviceHost.Open();

                Logger.Info( ServiceStatus.Running );
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
                if( ex.HResult == -2146233052 )
                    msg += "  Ensure the x86/x64 Sqlite folders are included with the distribution.";

                //_log.Write( Synapse.Common.LogLevel.Fatal, msg );
                Logger.Fatal( msg );
                WriteEventLog( msg );

                this.Stop();
                Environment.Exit( 99 );
            }
        }

        protected override void OnStop()
        {
            Logger.Info( ServiceStatus.Stopping );
            if( _serviceHost != null )
                _serviceHost.Close();
            Logger.Info( ServiceStatus.Stopped );
        }


        #region ensure database exists
        void EnsureDatabase()
        {
            Logger.Info( "EnsureDatabase: Checking file exists and connection is valid." );
            SynapseDal.CreateDatabase();
            Exception testResult = null;
            string message = string.Empty;
            if( !SynapseDal.TestConnection( out testResult, out message ) )
                throw testResult;
            Logger.Info( $"EnsureDatabase: Success. {message}" );
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
                Synapse.Common.SynapseLogManager _log = new Synapse.Common.SynapseLogManager( true );
                _log.Write( Synapse.Common.LogLevel.Fatal, msg );
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

        #region Help
        static void WriteHelpAndExit(string errorMessage = null)
        {
            bool haveError = !string.IsNullOrWhiteSpace( errorMessage );

            MessageBoxIcon icon = MessageBoxIcon.Information;
            string msg = $"synapse.service.exe, Version: {typeof( SynapseService ).Assembly.GetName().Version}\r\nSyntax:\r\n  synapse.service.exe /install | /uninstall";

            if( haveError )
            {
                msg += $"\r\n\r\n* Last error:\r\n{errorMessage}\r\nSee logs for details.";
                icon = MessageBoxIcon.Error;
            }

            MessageBox.Show( msg, "Synapse Server Service", MessageBoxButtons.OK, icon );

            Environment.Exit( haveError ? 1 : 0 );
        }
        #endregion
    }
}