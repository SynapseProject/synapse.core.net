using System;
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
			if( args.Length > 0 )
			{
				using( StreamReader sr = new StreamReader( @"yaml.yml" ) )
				{
					Plan plan = Plan.FromYaml( sr );
					Engine engine = new Engine();
					HandlerResult result = engine.Process( plan );
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

				using( StreamWriter file = new StreamWriter( @"yaml.yml" ) )
				{
					p.ToYaml( file );
				}
			}
			Console.Read();
		}
	}
}