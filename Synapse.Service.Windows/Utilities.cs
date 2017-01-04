using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;

namespace Synapse.Service.Windows
{
    public class InstallUtility
    {
        public static bool InstallService(bool install, out string message)
        {
            Type type = typeof( SynapseServiceInstaller );

            string logFile = $"Synapse.Service.InstallLog.txt";

            List<string> args = new List<string>();

            args.Add( $"/logfile={logFile}" );
            args.Add( "/LogToConsole=true" );
            args.Add( "/ShowCallStack=true" );
            args.Add( type.Assembly.Location );

            if( !install )
                args.Add( "/u" );

            try
            {
                ManagedInstallerClass.InstallHelper( args.ToArray() );
                message = "ok";
                return true;
            }
            catch( Exception ex )
            {
                string path = Path.GetDirectoryName( type.Assembly.Location );
                File.AppendAllText( $"{path}\\{logFile}", ex.Message );
                message = ex.Message;
                return false;
            }
        }
    }

    [RunInstaller( true )]
    public class SynapseServiceInstaller : Installer
    {
        public SynapseServiceInstaller()
        {
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = "Synapse Server";
            serviceInstaller.Description = "Synapse Server service.  Runs Plans, proxies to other Synapse Servers.  Use 'Synapse.Service /uninstall' to remove.  Information at http://synapse.readthedocs.io/en/latest/.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = "Synapse.Service";
            this.Installers.Add( processInstaller );
            this.Installers.Add( serviceInstaller );
        }
    }
}