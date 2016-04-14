using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core.Runtime
{
	public class Engine
	{
		public Engine() { }

		public void Process(Plan plan)
		{
			ProcessRecursive( plan.Actions, HandlerResult.Emtpy );
		}

		public void ProcessRecursive(List<ActionItem> actions, HandlerResult result)
		{
			IEnumerable<ActionItem> actionList = actions;
			if( !result.IsEmpty )
			{
				actionList = actions.Where( a => a.ResultCase == result.ExitCode );
			}
			foreach( ActionItem a in actionList )
			{
				string parms = GetParameters( a.Parameters );
				HandlerRuntime rt = HandlerRuntimeFactory.Create( a.Handler );
				HandlerResult r = rt.Execute( parms );
				if( a.HasActions )
				{
					ProcessRecursive( a.Actions, r );
				}
			}
		}

		string GetParameters(Parameters p)
		{
			string parms = string.Empty;
			if( p.HasValues )
			{
				parms = p.Values.ToString();
			}
			if( p.HasUri )
			{
				parms = string.Empty; //make rest call
				//merge parms
			}
			return parms;
		}
	}
}