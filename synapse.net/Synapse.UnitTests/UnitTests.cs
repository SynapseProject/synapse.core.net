using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

using Synapse.Core;
using Synapse.Core.Runtime;


namespace Synapse.UnitTests
{
    [TestFixture]
    public partial class UnitTests
    {
        static string __root = @"C:\Devo\synapse\synapse.core.net\synapse.net\Synapse.UnitTests";
        static string __work = $@"{__root}\bin\Debug";
        static string __plans = $@"{__root}\Plans";
        static string __config = $@"{__plans}\Config";
        static string __parms = $@"{__plans}\Parms";

        [OneTimeSetUp]
        public void Init()
        {
            Environment.CurrentDirectory = __root;
            System.IO.Directory.SetCurrentDirectory( __root );
        }

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Static" )]
        [TestCase( "parameters_yaml.yaml" )]
        public void MergeParameters_Static(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plans}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            // Assert
            string expectedMergeConfig = File.ReadAllText( $"{__config}\\yaml_out.yaml" );
            string actualMergedConfig = plan.Actions[0].Handler.Config.GetSerializedValues();

            Assert.AreEqual( expectedMergeConfig, actualMergedConfig );

            string expectedMergeParms = File.ReadAllText( $"{__parms}\\yaml_out.yaml" );
            string actualMergedParms = plan.Actions[0].Parameters.GetSerializedValues();

            Assert.AreEqual( expectedMergeParms, actualMergedParms );
        }

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Dynamic" )]
        [TestCase( "parameters_yaml.yaml" )]
        public void MergeParameters_Dynamic(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plans}\\{planFile}" );

            Dictionary<string, string> dynamicData = new Dictionary<string, string>();
            dynamicData.Add( "cnode0Dynamic", "CValue0_dynamic" );
            dynamicData.Add( "cnode2_1Dynamic", "CValue2_1_dynamic" );
            dynamicData.Add( "cnode3_1Dynamic", "CValue3_1_dynamic" );
            dynamicData.Add( "pnode0Dynamic", "PValue0_dynamic" );
            dynamicData.Add( "pnode2_1Dynamic", "PValue2_1_dynamic" );
            dynamicData.Add( "pnode3_1Dynamic", "PValue3_1_dynamic" );

            // Act
            plan.Start( dynamicData, true, true );

            // Assert
            string expectedMergeConfig = File.ReadAllText( $"{__config}\\yaml_out_dynamic.yaml" );
            string actualMergedConfig = plan.Actions[0].Handler.Config.GetSerializedValues();

            Assert.AreEqual( expectedMergeConfig, actualMergedConfig );

            string expectedMergeParms = File.ReadAllText( $"{__parms}\\yaml_out_dynamic.yaml" );
            string actualMergedParms = plan.Actions[0].Parameters.GetSerializedValues();

            Assert.AreEqual( expectedMergeParms, actualMergedParms );
        }
    }
}