using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core.Runtime
{
	public class Engine
	{
		Plan _p = null;

		public Engine(Plan plan)
		{
			_p = plan;
		}

		public void Process()
		{
			ProcessRecursive( _p.Actions, HandlerResult.Emtpy );
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
				HandlerRuntime rt = HandlerRuntimeFactory.Create( a.Handler );
				HandlerResult r = rt.Execute();
				if( a.HasActions )
				{
					ProcessRecursive( a.Actions, r );
				}
			}
		}
	}
}