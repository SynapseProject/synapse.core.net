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

        [Test]
        [Category( "Service" )]
        [TestCase( 4 )]
        public void PlanScheduler(int maxThreads)
        {
            Synapse.Core.DataAccessLayer.SynapseDal.CreateDatabase();

            string plan0Name = "planScheduler.yaml";
            string plan1Name = "planScheduler.yaml";
            string plan2Name = "planScheduler.yaml";
            string plan3Name = "planScheduler.yaml";

            Plan plan0 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
            Plan plan1 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
            Plan plan2 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
            Plan plan3 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );
            Plan plan4 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
            Plan plan5 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
            Plan plan6 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
            Plan plan7 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );

            PlanRuntimePod p0 = new PlanRuntimePod( plan0 );
            PlanRuntimePod p1 = new PlanRuntimePod( plan1 );
            PlanRuntimePod p2 = new PlanRuntimePod( plan2 );
            PlanRuntimePod p3 = new PlanRuntimePod( plan3 );
            PlanRuntimePod p4 = new PlanRuntimePod( plan4 );
            PlanRuntimePod p5 = new PlanRuntimePod( plan5 );
            PlanRuntimePod p6 = new PlanRuntimePod( plan6 );
            PlanRuntimePod p7 = new PlanRuntimePod( plan7 );

            PlanScheduler scheduler = new PlanScheduler( maxThreads );
            scheduler.StartPlan( p0 );
            scheduler.StartPlan( p1 );
            scheduler.StartPlan( p2 );
            scheduler.StartPlan( p3 );
            scheduler.StartPlan( p4 );
            scheduler.StartPlan( p5 );
            scheduler.StartPlan( p6 );
            scheduler.StartPlan( p7 );
            scheduler.Drainstop();

            File.WriteAllText( $"{__plansOut}\\plan0_out.yaml", plan0.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan1_out.yaml", plan1.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan2_out.yaml", plan2.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan3_out.yaml", plan3.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan4_out.yaml", plan4.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan5_out.yaml", plan5.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan6_out.yaml", plan6.ResultPlan.ToYaml() );
            File.WriteAllText( $"{__plansOut}\\plan7_out.yaml", plan7.ResultPlan.ToYaml() );

            if( scheduler.IsDrainstopped )
            {
                scheduler.Undrainstop();

                Plan plan00 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
                Plan plan01 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
                Plan plan02 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
                Plan plan03 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );
                Plan plan04 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
                Plan plan05 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
                Plan plan06 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
                Plan plan07 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );

                PlanRuntimePod p00 = new PlanRuntimePod( plan00 );
                PlanRuntimePod p01 = new PlanRuntimePod( plan01 );
                PlanRuntimePod p02 = new PlanRuntimePod( plan02 );
                PlanRuntimePod p03 = new PlanRuntimePod( plan03 );
                PlanRuntimePod p04 = new PlanRuntimePod( plan04 );
                PlanRuntimePod p05 = new PlanRuntimePod( plan05 );
                PlanRuntimePod p06 = new PlanRuntimePod( plan06 );
                PlanRuntimePod p07 = new PlanRuntimePod( plan07 );

                scheduler.StartPlan( p00 );
                scheduler.StartPlan( p01 );
                scheduler.StartPlan( p02 );
                scheduler.StartPlan( p03 );
                scheduler.StartPlan( p04 );
                scheduler.StartPlan( p05 );
                scheduler.StartPlan( p06 );
                scheduler.StartPlan( p07 );
                scheduler.Drainstop();

                File.WriteAllText( $"{__plansOut}\\plan00_out.yaml", plan00.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan01_out.yaml", plan01.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan02_out.yaml", plan02.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan03_out.yaml", plan03.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan04_out.yaml", plan04.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan05_out.yaml", plan05.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan06_out.yaml", plan06.ResultPlan.ToYaml() );
                File.WriteAllText( $"{__plansOut}\\plan07_out.yaml", plan07.ResultPlan.ToYaml() );
            }
            else
            {
                Assert.IsTrue( scheduler.IsDrainstopped );
            }

            Assert.IsTrue( scheduler.IsDrainstopped );
        }
    }
}