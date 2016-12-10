using System;
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

            if( _serviceHost != null )
            {
                _serviceHost.Close();
            }

            _serviceHost = new ServiceHost( typeof( SynapseServer ) );
            _serviceHost.Open();

            _log.Write( ServiceStatus.Running );
        }

        protected override void OnStop()
        {
            _log.Write( ServiceStatus.Stopping );
            _serviceHost.Close();
            _log.Write( ServiceStatus.Stopped );
        }
    }
}