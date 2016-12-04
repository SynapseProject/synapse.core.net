﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        static string __plansRoot = $@"{__root}\Plans";
        static string __plansOut = $@"{__plansRoot}\Plans";
        static string __config = $@"{__plansRoot}\Config";
        static string __parms = $@"{__plansRoot}\Parms";

        [OneTimeSetUp]
        public void Init()
        {
            Environment.CurrentDirectory = __root;
            System.IO.Directory.SetCurrentDirectory( __root );
        }

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Static" )]
        [TestCase( "parameters_yaml_single.yaml" )]
        public void MergeParameters_Static(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

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
        [TestCase( "parameters_yaml_single.yaml" )]
        public void MergeParameters_Dynamic(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

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

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Dynamic" )]
        [Category( "Parameters_ForEach" )]
        [TestCase( "parameters_yaml_foreach.yaml" )]
        public void MergeParameters_ForEach(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            Dictionary<string, string> dynamicData = new Dictionary<string, string>();
            dynamicData.Add( "cnode0Dynamic", "CValue0_dynamic" );
            dynamicData.Add( "cnode2_1Dynamic", "CValue2_1_dynamic" );
            dynamicData.Add( "cnode3_1Dynamic", "CValue3_1_dynamic" );
            dynamicData.Add( "pnode0Dynamic", "PValue0_dynamic" );
            dynamicData.Add( "pnode2_1Dynamic", "PValue2_1_dynamic" );
            dynamicData.Add( "pnode3_1Dynamic", "PValue3_1_dynamic" );

            //List<ActionItem> resolvedActions = new List<ActionItem>();
            //ActionItem ai = plan.Actions[0];
            //ai.ResolveConfigAndParameters( dynamicData, null, null, ref resolvedActions );

            // Act
            plan.Start( dynamicData, true, true );

            // Assert
            List<int> configKeys = new List<int>();
            Dictionary<int, string> configHashes = new Dictionary<int, string>();
            List<int> parmKeys = new List<int>();
            Dictionary<int, string> parmHashes = new Dictionary<int, string>();
            List<int> actionKeys = new List<int>();
            Dictionary<int, string> actionHashes = new Dictionary<int, string>();
            StringBuilder actualMergedConfig = new StringBuilder();
            StringBuilder actualMergedParms = new StringBuilder();
            StringBuilder actualMergedActions = new StringBuilder();
            foreach( ActionItem resolvedAction in plan.ResultPlan.Actions )
            {
                string config = resolvedAction.Handler.Config.GetSerializedValues();
                int hash = config.GetHashCode();
                if( !configHashes.ContainsKey( hash ) )
                {
                    configKeys.Add( hash );
                    configHashes.Add( hash, config );
                }

                string parms = resolvedAction.Parameters.GetSerializedValues();
                hash = parms.GetHashCode();
                if( !parmHashes.ContainsKey( hash ) )
                {
                    parmKeys.Add( hash );
                    parmHashes.Add( hash, parms );
                }

                string actionData = config + parms;
                hash = actionData.GetHashCode();
                if( !actionHashes.ContainsKey( hash ) )
                {
                    actionKeys.Add( hash );
                    actionHashes.Add( hash, actionData );
                }
            }

            configKeys.Sort();
            foreach( int key in configKeys )
                actualMergedConfig.AppendLine( configHashes[key] );
            parmKeys.Sort();
            foreach( int key in parmKeys )
                actualMergedParms.AppendLine( parmHashes[key] );
            actionKeys.Sort();
            foreach( int key in actionKeys )
            {
                actualMergedActions.AppendLine( actionHashes[key] );
                actualMergedActions.AppendLine( "--------------------\r\n" );
            }


            string expectedMergeConfig = File.ReadAllText( $"{__config}\\yaml_out_dynamic_foreach_plan.yaml" );
            Assert.AreEqual( expectedMergeConfig, actualMergedConfig.ToString() );

            string expectedMergeParms = File.ReadAllText( $"{__parms}\\yaml_out_dynamic_foreach_plan.yaml" );
            Assert.AreEqual( expectedMergeParms, actualMergedParms.ToString() );

            string expectedMergeActions = File.ReadAllText( $"{__plansOut}\\parameters_yaml_foreach_out.yaml" );
            Assert.AreEqual( expectedMergeActions, actualMergedActions.ToString() );
        }
    }
}