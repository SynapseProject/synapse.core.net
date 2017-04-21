using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Synapse.Core;
using Synapse.Core.DataAccessLayer;
using Synapse.Core.Runtime;
using Synapse.Core.Utilities;

namespace Synapse.Tester
{
    class Program
    {
        static int _count = 0;
        static Plan plan = null;
        static void Main(string[] args)
        {
            //EnsureDatabaseExists();


            //string __root = @"C:\Devo\synapse\synapse.core.net\Synapse.UnitTests";
            //string __plansRoot = $@"{__root}\Plans";
            //string __plansOut = $@"{__plansRoot}\Plans";
            //string plan0Name = "statusPropagation_single.yaml";
            //string plan1Name = "statusPropagation_single_actionGroup.yaml";
            //string plan2Name = "statusPropagation_forEach.yaml";
            //string plan3Name = "statusPropagation_forEach_actionGroup.yaml";

            //Plan plan0 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
            //Plan plan1 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
            //Plan plan2 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
            //Plan plan3 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );

            //PlanScheduler scheduler = new PlanScheduler( 10 );
            //scheduler.StartPlan( "0", false, plan0 );
            //scheduler.StartPlan( "1", false, plan1 );
            //scheduler.StartPlan( "2", false, plan2 );
            //scheduler.StartPlan( "3", false, plan3 );
            //scheduler.Drainstop();

            //File.WriteAllText( $"{__plansOut}\\{plan0.Name}_out.yaml", plan0.ResultPlan.ToYaml() );
            //File.WriteAllText( $"{__plansOut}\\{plan1.Name}_out.yaml", plan1.ResultPlan.ToYaml() );
            //File.WriteAllText( $"{__plansOut}\\{plan2.Name}_out.yaml", plan2.ResultPlan.ToYaml() );
            //File.WriteAllText( $"{__plansOut}\\{plan3.Name}_out.yaml", plan3.ResultPlan.ToYaml() );

            //Plan plan00 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
            //Plan plan01 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
            //Plan plan02 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
            //Plan plan03 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );

            ////plan0 = plan1 = plan2 = plan3 = null;

            //scheduler.StartPlan( "0", false, plan00 );
            //scheduler.StartPlan( "1", false, plan01 );
            //scheduler.StartPlan( "2", false, plan02 );
            //scheduler.StartPlan( "3", false, plan03 );
            //scheduler.Drainstop();

            //File.WriteAllText( $"{__plansOut}\\{plan00.Name}_out.yaml", plan00.ResultPlan.ToYaml() );
            //File.WriteAllText( $"{__plansOut}\\{plan01.Name}_out.yaml", plan01.ResultPlan.ToYaml() );
            //File.WriteAllText( $"{__plansOut}\\{plan02.Name}_out.yaml", plan02.ResultPlan.ToYaml() );
            //File.WriteAllText( $"{__plansOut}\\{plan03.Name}_out.yaml", plan03.ResultPlan.ToYaml() );

            //Environment.Exit( 0 );



            Plan guy = Plan.FromYaml( @"..\..\yaml\ad-test.yaml" );

            Plan encrypted = guy.EncryptElements();
            YamlHelpers.SerializeFile( @"..\..\yaml\ad-test.encr.yaml", encrypted );
            guy = encrypted.DecryptElements();


            Dictionary<string, string> p = new Dictionary<string, string>();
            p.Add( "jsonPayload", "{ GroupName: \"MyNewGroup\", Users: [ \"Guy Michael Waguespack\", \"Steven James Shortt\", \"Kitten Foo\", \"Matthew Paige Damon\" ] }" );
            p.Add( "prop0", "value0" );
            p.Add( "prop1", "value1" );
            p.Add( "prop2", "value2" );
            p.Add( "prop3", "value3" );
            p.Add( "prop4", "value4" );
            guy.Start( p );
            string rp = guy.ResultPlan.ToYaml();
            Environment.Exit( 0 );



            string path = @"..\..\yaml\example.yml";
            string outpath = @"..\..\yaml\example.out.yml";
            if( args.Length > 0 )
            {
                Dictionary<string, string> parms = new Dictionary<string, string>();
                parms["app"] = "someApp";
                parms["type"] = "someType";

                using( StreamReader sr = new StreamReader( path ) )
                    plan = Plan.FromYaml( sr );

                plan.Actions[0].ActionGroup = plan.Actions[0].Clone();
                using( StreamWriter file = new StreamWriter( outpath ) )
                    plan.ToYaml( file );

                plan.Progress += plan_Progress;

                //PlanScheduler sch = new PlanScheduler( 5 );
                //sch.StartPlan( "1", false, plan );

                //HandlerResult result = plan.Start( parms, dryRun: false );
                //Console.WriteLine( result );
                using( StreamWriter file = new StreamWriter( outpath ) )
                    plan.ToYaml( file );
            }
            else
            {
                ActionItem ac2 = ActionItem.CreateDummy( "ac2" );
                ActionItem ac1 = ActionItem.CreateDummy( "ac1" );
                ActionItem ac0 = ActionItem.CreateDummy( "ac0" );
                ac0.Actions = new List<ActionItem>();
                ac0.Actions.Add( ac1 );
                List<ActionItem> actions = new List<ActionItem>();
                actions.Add( ac0 );
                actions.Add( ac2 );

                plan = new Plan()
                {
                    Name = "plan0",
                    Actions = actions,
                    Description = "planDesc",
                    IsActive = true
                };

                using( StreamWriter file = new StreamWriter( path ) )
                {
                    plan.ToYaml( file );
                }
            }
            Console.Read();
        }

        static void plan_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            if( e.ActionName == "ac0.1.1" && e.Status == StatusType.Initializing )
            {
                plan.Pause();
                System.Threading.Thread.Sleep( 5000 );
                plan.Continue();
            }
            else if( e.ActionName == "ac0.1.2" && e.Status == StatusType.Initializing )
            {
                e.Cancel = true;
            }
            //Console.WriteLine( "ActionName: {0}, Context:{1}, Message:{2}, StatusType:{3}", e.ActionName, e.Context, e.Message, e.Status );
            Console.WriteLine( "ActionName: {0}, Context:{1}, Message:{2}, StatusType:{3}, {4}", e.ActionName, e.Context, e.Message, e.Status, ++_count );
        }


        #region ensure database exists
        static void EnsureDatabaseExists()
        {
            try
            {
                Synapse.Core.DataAccessLayer.SynapseDal.CreateDatabase();
            }
            catch( Exception ex )
            {
                ConsoleColor defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                if( ex.HResult == -2146233052 )
                {
                    Console.WriteLine( ex.Message );
                    Console.WriteLine( "Ensure the x86/x64 Sqlite folders are included with the distribution." );
                }
                else
                {
                    Console.WriteLine( ex.Message );
                }
                Console.ForegroundColor = defaultColor;
                Environment.Exit( 99 );
            }
        }
        #endregion
    }
}