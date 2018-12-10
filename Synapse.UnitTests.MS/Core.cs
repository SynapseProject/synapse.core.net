using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Synapse.Core;

namespace Synapse.UnitTests.MS
{
    [TestClass]
    public class Core
    {
        static string __root = @"C:\Devo\synapse\synapse.core.net\Synapse.UnitTests";
        static string __work = $@"{__root}\bin\Debug";
        static string __plansRoot = $@"{__root}\Plans";
        static string __plansOut = $@"{__plansRoot}\Plans";
        static string __config = $@"{__plansRoot}\Config";
        static string __parms = $@"{__plansRoot}\Parms";
        static string __crypto = $@"{__plansRoot}\crypto";

        [DataTestMethod]
        [TestCategory( "Parameters" )]
        [TestCategory( "Parameters_Static" )]
        [DataRow( "parameters_yaml_single.yaml" )]
        [DataRow( "parameters_json_single.yaml" )]
        [DataRow( "parameters_xml_single.yaml" )]
        public void MergeParameters_Static(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );

            // Act
            plan.Start( null, true, true );

            //// Assert
            //string type = plan.Actions[0].Parameters.Type.ToString().ToLower();

            //string expectedMergeConfig = File.ReadAllText( $"{__config}\\{type}_out.{type}" ); ;
            //string actualMergedConfig = plan.Actions[0].Handler.Config.GetSerializedValues();
            ////File.WriteAllText( $"{__config}\\{type}_out.{type}", actualMergedConfig );
            //Assert.AreEqual( expectedMergeConfig, actualMergedConfig );

            //string expectedMergeParms = File.ReadAllText( $"{__parms}\\{type}_out.{type}" ); ;
            //string actualMergedParms = plan.Actions[0].Parameters.GetSerializedValues();
            ////File.WriteAllText( $"{__parms}\\{type}_out.{type}", actualMergedParms );
            //Assert.AreEqual( expectedMergeParms, actualMergedParms );
        }


        [DataTestMethod]
        [TestCategory( "Parameters" )]
        [TestCategory( "Parameters_Validation" )]
        [DataRow( "parameters_yaml_validation.yaml" )]
        [DataRow( "parameters_xml_validation.yaml" )]
        public void ValidateParameters_Static(string planFile)
        {
            // Arrange
            Plan plan = Plan.FromYaml( $"{__plansRoot}\\{planFile}" );
            Dictionary<string, string> parms = new Dictionary<string, string>
            {
                { "sleep0", "2001" },
                { "sleep1", "2001" },
                { "sleep2", "2001" },
                { "sleep3", "2000" }
            };

            // Act
            plan.Start( parms, true, true );

            // Assert
            string expected = "DynamicValue [2001] failed validation rule type requirement of [DateTime] for parameter [sleep0].";
            ActionItem action = plan.ResultPlan.Actions.Single( a => a.Name.Equals( "action0", StringComparison.OrdinalIgnoreCase ) );
            bool found = action.Result.Message.Contains( expected );
            Assert.IsTrue( found );

            expected = "DynamicValue [2001] failed validation rule [^([1-9]|[01][0-9][0-9]|200[0-0])$] for parameter [sleep1].";
            action = plan.ResultPlan.Actions.Single( a => a.Name.Equals( "action1", StringComparison.OrdinalIgnoreCase ) );
            found = action.Result.Message.Contains( expected );
            Assert.IsTrue( found );

            expected = "DynamicValue [2001] failed validation rule [RestrictToOptions] for parameter [sleep2].";
            action = plan.ResultPlan.Actions.Single( a => a.Name.Equals( "action2", StringComparison.OrdinalIgnoreCase ) );
            found = action.Result.Message.Contains( expected );
            Assert.IsTrue( found );

            expected = "EmptyHandler ExitData default value.";
            action = plan.ResultPlan.Actions.Single( a => a.Name.Equals( "action3", StringComparison.OrdinalIgnoreCase ) );
            found = action.Result.ExitData.ToString().Contains( expected );
            Assert.IsTrue( found );
        }


    }
}
