using System;
using System.Collections.Generic;
using System.Configuration.Install;

namespace Synapse.Service.Windows
{
	class Utilities
	{
		public static void Install(Type type, bool install)
		{
			List<string> installArgs = new List<string>();
			installArgs.Add( string.Format( "/logfile={0}.installLog.txt", type.FullName ) );
			installArgs.Add( "/LogToConsole=true" );
			installArgs.Add( "/ShowCallStack=true" );
			installArgs.Add( type.Assembly.Location );
			if( !install )
			{
				installArgs.Add( "/u" );
			}
			ManagedInstallerClass.InstallHelper( installArgs.ToArray() );
		}
	}
}
