using System;
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
        static string __root = @"C:\Devo\synapse\synapse.core.net\Synapse.UnitTests";
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
        [TestCase( "parameters_json_single.yaml" )]
        [TestCase( "parameters_xml_single.yaml" )]
        public void MergeParameters_Static(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            // Assert
            bool isYamlJson = plan.Actions[0].Parameters.Type == SerializationType.Yaml ||
                plan.Actions[0].Parameters.Type == SerializationType.Json;

            string expectedMergeConfig = null;
            string actualMergedConfig = plan.Actions[0].Handler.Config.GetSerializedValues();
            if( isYamlJson )
                expectedMergeConfig = File.ReadAllText( $"{__config}\\yaml_out.yaml" );
            else
                expectedMergeConfig = File.ReadAllText( $"{__config}\\xml_out.xml" );

            Assert.AreEqual( expectedMergeConfig, actualMergedConfig );

            string expectedMergeParms = null;
            string actualMergedParms = plan.Actions[0].Parameters.GetSerializedValues();
            if( isYamlJson )
                expectedMergeParms = File.ReadAllText( $"{__parms}\\yaml_out.yaml" );
            else
                expectedMergeParms = File.ReadAllText( $"{__parms}\\xml_out.xml" );

            Assert.AreEqual( expectedMergeParms, actualMergedParms );
        }

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Dynamic" )]
        [TestCase( "parameters_yaml_single.yaml" )]
        [TestCase( "parameters_json_single.yaml" )]
        [TestCase( "parameters_xml_single.yaml" )]
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
            bool isYamlJson = plan.Actions[0].Parameters.Type == SerializationType.Yaml ||
                plan.Actions[0].Parameters.Type == SerializationType.Json;

            string expectedMergeConfig = null;
            string actualMergedConfig = plan.Actions[0].Handler.Config.GetSerializedValues();
            if( isYamlJson )
                expectedMergeConfig = File.ReadAllText( $"{__config}\\yaml_out_dynamic.yaml" );
            else
                expectedMergeConfig = File.ReadAllText( $"{__config}\\xml_out_dynamic.xml" );

            Assert.AreEqual( expectedMergeConfig, actualMergedConfig );

            string expectedMergeParms = null;
            string actualMergedParms = plan.Actions[0].Parameters.GetSerializedValues();
            if( isYamlJson )
                expectedMergeParms = File.ReadAllText( $"{__parms}\\yaml_out_dynamic.yaml" );
            else
                expectedMergeParms = File.ReadAllText( $"{__parms}\\xml_out_dynamic.xml" );

            Assert.AreEqual( expectedMergeParms, actualMergedParms );
        }

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Inherit" )]
        [TestCase( "parameters_yaml_single.yaml" )]
        [TestCase( "parameters_json_single.yaml" )]
        [TestCase( "parameters_xml_single.yaml" )]
        public void MergeParameters_Inherit(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            // Assert
            bool isYamlJson = plan.Actions[0].Parameters.Type == SerializationType.Yaml ||
                plan.Actions[0].Parameters.Type == SerializationType.Json;

            string expectedMergeConfig = null;
            string actualMergedConfig = plan.Actions[0].Actions[0].Handler.Config.GetSerializedValues();
            if( isYamlJson )
                expectedMergeConfig = File.ReadAllText( $"{__config}\\yaml_out_inherit.yaml" );
            else
                expectedMergeConfig = File.ReadAllText( $"{__config}\\xml_out_inherit.xml" );

            Assert.AreEqual( expectedMergeConfig, actualMergedConfig );

            string expectedMergeParms = null;
            string actualMergedParms = plan.Actions[0].Actions[0].Parameters.GetSerializedValues();
            if( isYamlJson )
                expectedMergeParms = File.ReadAllText( $"{__parms}\\yaml_out_inherit.yaml" );
            else
                expectedMergeParms = File.ReadAllText( $"{__parms}\\xml_out_inherit.xml" );

            Assert.AreEqual( expectedMergeParms, actualMergedParms );
        }

        [Test]
        [Category( "Parameters" )]
        [Category( "Parameters_Dynamic" )]
        [Category( "Parameters_ForEach" )]
        [TestCase( "parameters_yaml_foreach.yaml" )]
        [TestCase( "parameters_json_foreach.yaml" )]
        [TestCase( "parameters_xml_foreach.yaml" )]
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
            bool isYamlJson = plan.Actions[0].Parameters.Type == SerializationType.Yaml ||
                plan.Actions[0].Parameters.Type == SerializationType.Json;

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

                string actionData = config + "\r\n" + parms;
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

            string lang = isYamlJson ? "yaml" : "xml";
            string expectedMergeConfig = File.ReadAllText( $"{__config}\\{lang}_out_dynamic_foreach_plan.{lang}" );
            Assert.AreEqual( expectedMergeConfig, actualMergedConfig.ToString() );

            string expectedMergeParms = File.ReadAllText( $"{__parms}\\{lang}_out_dynamic_foreach_plan.{lang}" );
            Assert.AreEqual( expectedMergeParms, actualMergedParms.ToString() );

            string expectedMergeActions = File.ReadAllText( $"{__plansOut}\\parameters_{lang}_foreach_out.{lang}" );
            Assert.AreEqual( expectedMergeActions, actualMergedActions.ToString() );
        }

        [Test]
        [Category( "Handlers" )]
        [TestCase( "handlerLoad.yaml" )]
        public void LoadHandlers(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            // Assert
            string expectedType = typeof( Synapse.Core.Runtime.EmptyHandler ).AssemblyQualifiedName;
            Assert.AreEqual( expectedType, plan.ResultPlan.Actions[0].Handler.Type );
            Assert.AreEqual( expectedType, plan.ResultPlan.Actions[0].Actions[0].Handler.Type );
            Assert.AreEqual( expectedType, plan.ResultPlan.Actions[0].Actions[0].Actions[0].Handler.Type );
            Assert.AreEqual( expectedType, plan.ResultPlan.Actions[0].Actions[0].Actions[0].Actions[0].Handler.Type );
            Assert.AreEqual( expectedType, plan.ResultPlan.Actions[0].Actions[0].Actions[0].Actions[0].Actions[0].Handler.Type );
        }


        [Test]
        [Category( "Status" )]
        [TestCase( "statusPropagation_single.yaml" )]
        [TestCase( "statusPropagation_single_actionGroup.yaml" )]
        public void StatusPropagation_Single(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            // Assert
            Dictionary<string, StatusType> expectedStatus = new Dictionary<string, StatusType>();
            Dictionary<string, StatusType> actualStatus = new Dictionary<string, StatusType>();

            StatusType maxStatus = StatusType.None;

            Stack<List<ActionItem>> actionLists = new Stack<List<ActionItem>>();
            actionLists.Push( plan.Actions );
            while( actionLists.Count > 0 )
            {
                List<ActionItem> actions = actionLists.Pop();
                foreach( ActionItem a in actions )
                {
                    expectedStatus.Add( a.Name, StatusType.None );
                    if( a.HasParameters && a.Parameters.HasValues )
                        expectedStatus[a.Name] = (StatusType)Enum.Parse( typeof( StatusType ),
                            ((Dictionary<object, object>)a.Parameters.Values)["ReturnStatus"].ToString() );
                    if( a.HasActionGroup )
                        actionLists.Push( new List<ActionItem>( new ActionItem[] { a.ActionGroup } ) );
                    if( a.HasActions )
                        actionLists.Push( a.Actions );

                    if( (int)expectedStatus[a.Name] > (int)maxStatus )
                        maxStatus = expectedStatus[a.Name];
                }
            }
            actionLists.Push( plan.ResultPlan.Actions );
            while( actionLists.Count > 0 )
            {
                List<ActionItem> actions = actionLists.Pop();
                foreach( ActionItem a in actions )
                {
                    actualStatus.Add( a.Name, a.Result.Status );
                    if( a.HasActionGroup )
                        actionLists.Push( new List<ActionItem>( new ActionItem[] { a.ActionGroup } ) );
                    if( a.HasActions )
                        actionLists.Push( a.Actions );
                }
            }

            string planResult = plan.ResultPlan.ToYaml();
            File.WriteAllText( $"{__plansOut}\\{plan.Name}_out.yaml", plan.ResultPlan.ToYaml() );

            Assert.AreEqual( maxStatus, plan.Result.Status );
            Assert.AreEqual( expectedStatus.Count, actualStatus.Count );
            foreach( string key in expectedStatus.Keys )
                Assert.AreEqual( expectedStatus[key], actualStatus[key] );
        }

        [Test]
        [Category( "Status" )]
        [TestCase( "statusPropagation_forEach.yaml" )]
        [TestCase( "statusPropagation_forEach_actionGroup.yaml" )]
        public void StatusPropagation_ForEach(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            string planResult = plan.ResultPlan.ToYaml();
            File.WriteAllText( $"{__plansOut}\\{plan.Name}_out.yaml", plan.ResultPlan.ToYaml() );
        }
    }
}