using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

using Synapse.Core;
using Synapse.Core.Runtime;
using Synapse.Service.Windows;

namespace Synapse.UnitTests
{
    public partial class UnitTests
    {
        [Test]
        [Category( "Service" )]
        [Category( "ServiceControl" )]
        public void ServiceInstaller()
        {
            // Arrange

            // Act
            bool ok = true;
            string message = string.Empty;

            //make sure the service isn't there
            ok = InstallUtility.InstallService( install: false, message: out message );


            //in each test/assert below, the service shoud succeed, then fail due to duplicate action

            //install
            ok = InstallUtility.InstallService( install: true, message: out message );
            Assert.IsTrue( ok );
            //repeat install (should fail)
            ok = InstallUtility.InstallService( install: true, message: out message );
            Assert.IsFalse( ok );

            //uninstall
            ok = InstallUtility.InstallService( install: false, message: out message );
            Assert.IsTrue( ok );
            //repeat uninstall (should fail)
            ok = InstallUtility.InstallService( install: false, message: out message );
            Assert.IsFalse( ok );
        }
    }
}