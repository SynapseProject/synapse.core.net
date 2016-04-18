using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core.Runtime
{
	public class Engine
	{
		Dal.DataAccessLayer _dal = new Dal.DataAccessLayer();

		public Engine() { }

		public HandlerResult Process(Plan plan)
		{
			return ProcessRecursive( plan.Actions, HandlerResult.Emtpy );
		}

		HandlerResult ProcessRecursive(List<ActionItem> actions, HandlerResult result)
		{
			HandlerResult returnResult = HandlerResult.Emtpy;
			IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == result.Status );

			//multithread this with task.parallel
			foreach( ActionItem a in actionList )
			{
				string parms = a.HasParameters ? a.Parameters.Resolve() : null;
				IHandlerRuntime rt = HandlerRuntimeFactory.Create( a.Handler );
				HandlerResult r = rt.Execute( parms );
				_dal.UpdateActionStatus( a, r );

				if( r.Status > returnResult.Status ) { returnResult = r; }

				if( a.HasActions )
				{
					r = ProcessRecursive( a.Actions, r );
					if( r.Status > returnResult.Status ) { returnResult = r; }
				}
			}

			return returnResult;
		}
	}
}