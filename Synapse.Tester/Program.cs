﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Synapse.Core;
using Synapse.Core.Runtime;


namespace Synapse.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			string path = @"..\..\yaml\example.yml";
			string outpath = @"..\..\yaml\example.out.yml";
			if( args.Length > 0 )
			{
				Dictionary<string, string> parms = new Dictionary<string, string>();
				parms["app"] = "someApp";
				parms["type"] = "someType";
				Plan plan = null;
				using( StreamReader sr = new StreamReader( path ) )
				{
					plan = Plan.FromYaml( sr );
				}
				plan.Progress += plan_Progress;
				HandlerResult result = plan.Start( parms );
				Console.WriteLine( result );
				using( StreamWriter file = new StreamWriter( outpath ) )
				{
					plan.ToYaml( file );
				}
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

				Plan p = new Plan()
				{
					Name = "plan0",
					Actions = actions,
					Description = "planDesc",
					IsActive = true
				};

				using( StreamWriter file = new StreamWriter( path ) )
				{
					p.ToYaml( file );
				}
			}
			Console.Read();
		}

		static void plan_Progress(object sender, HandlerProgressCancelEventArgs e)
		{
			if( e.ActionName == "ac0.1.3" && e is HandlerProgressCancelEventArgs )
			{
				((HandlerProgressCancelEventArgs)e).Cancel = true;
			}
			Console.WriteLine( "ActionName: {0}, Context:{1}, Message:{2}", e.ActionName, e.Context, e.Message );
		}
	}
}