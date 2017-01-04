using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

class InstallUtility
{
    public static void InstallService(bool install)
    {
        Type type = typeof( SynapseServiceInstaller );

        List<string> args = new List<string>();
        args.Add( string.Format( "/logfile={0}.installLog.txt", type.FullName ) );
        args.Add( "/LogToConsole=true" );
        args.Add( "/ShowCallStack=true" );
        args.Add( type.Assembly.Location );

        if( !install )
            args.Add( "/u" );

        ManagedInstallerClass.InstallHelper( args.ToArray() );
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

        serviceInstaller.DisplayName = "Synapse.Service";
        serviceInstaller.Description = "Synapse Server service.  Runs Plans, proxies to other Synapse Servers. Use 'Synapse.Service /uninstall' to remove.  Information at http://synapse.readthedocs.io/en/latest/.";
        serviceInstaller.StartType = ServiceStartMode.Automatic;

        //must be the same as what was set in Program's constructor
        serviceInstaller.ServiceName = "Synapse.Service";
        this.Installers.Add( processInstaller );
        this.Installers.Add( serviceInstaller );
    }
}