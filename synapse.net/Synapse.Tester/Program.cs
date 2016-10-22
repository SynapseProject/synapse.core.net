using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Synapse.Core;
using Synapse.Core.DataAccessLayer;
using Synapse.Core.Runtime;


namespace Synapse.Tester
{
    class Program
    {
        static int _count = 0;
        static Plan plan = null;
        static void Main(string[] args)
        {
            SynapseDal dal = new SynapseDal();
            dal.CreateDatabase();

            string path = @"..\..\yaml\example.yml";
            string outpath = @"..\..\yaml\example.out.yml";
            if( args.Length > 0 )
            {
                Dictionary<string, string> parms = new Dictionary<string, string>();
                parms["app"] = "someApp";
                parms["type"] = "someType";

                using( StreamReader sr = new StreamReader( path ) )
                    plan = Plan.FromYaml( sr );

                dal.CreatePlanInstance( ref plan );

                plan.Actions[0].ActionGroup = plan.Actions[0].Clone();
                using( StreamWriter file = new StreamWriter( outpath ) )
                    plan.ToYaml( file );

                plan.Progress += plan_Progress;

                PlanScheduler sch = new PlanScheduler( 5 );
                sch.StartPlan( "1", false, plan );

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
    }
}