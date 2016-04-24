using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core.Runtime
{
	//todo: multithread this with task.parallel, ensure thread safety on dicts
	public class Engine
	{
		Dictionary<string, Config> _configSets = new Dictionary<string, Config>();
		Dictionary<string, Parameters> _paramSets = new Dictionary<string, Parameters>();

		public Engine() { }

		public HandlerResult Process(Plan plan, Dictionary<string, string> dynamicData)
		{
			return ProcessRecursive( plan.Actions, HandlerResult.Emtpy, dynamicData );
		}

		HandlerResult ProcessRecursive(List<ActionItem> actions, HandlerResult result, Dictionary<string, string> dynamicData)
		{
			HandlerResult returnResult = HandlerResult.Emtpy;
			IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == result.Status );

			foreach( ActionItem a in actionList )
			{
				string parms = ResolveConfigAndParameters( a, dynamicData );

				IHandlerRuntime rt = HandlerRuntimeFactory.Create( a.Handler );
				HandlerResult r = rt.Execute( parms );

				if( r.Status > returnResult.Status ) { returnResult = r; }

				if( a.HasActions )
				{
					r = ProcessRecursive( a.Actions, r, dynamicData );
					if( r.Status > returnResult.Status ) { returnResult = r; }
				}
			}

			return returnResult;
		}

		string ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData)
		{
			if( a.Handler.HasConfig )
			{
				Config c = a.Handler.Config;
				if( c.HasInheritFrom && _configSets.Keys.Contains( c.InheritFrom ) )
				{
					c.InheritedValues = _configSets[c.InheritFrom];
				}
				c.Resolve( dynamicData );

				if( c.HasName )
				{
					_configSets[c.Name] = c;
				}
			}

			string parms = null;
			if( a.HasParameters )
			{
				Parameters p = a.Parameters;
				if( p.HasInheritFrom && _paramSets.Keys.Contains( p.InheritFrom ) )
				{
					p.InheritedValues = _paramSets[p.InheritFrom];
				}
				parms = p.Resolve( dynamicData );

				if( p.HasName )
				{
					_paramSets[p.Name] = p;
				}
			}

			return parms;
		}
	}
}