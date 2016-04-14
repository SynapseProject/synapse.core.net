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
		public Engine() { }

		public HandlerResult Process(Plan plan)
		{
			return ProcessRecursive( plan.Actions, HandlerResult.Emtpy );
		}

		HandlerResult ProcessRecursive(List<ActionItem> actions, HandlerResult result)
		{
			HandlerResult r = HandlerResult.Emtpy;
			IEnumerable<ActionItem> actionList = actions.Where( a => a.ResultCase == result.ExitCode );

			//multithread this with task.parallel
			foreach( ActionItem a in actionList )
			{
				string parms = GetParameters( a.Parameters );
				IHandlerRuntime rt = HandlerRuntimeFactory.Create( a.Handler );
				r = rt.Execute( parms );
				if( a.HasActions )
				{
					r = ProcessRecursive( a.Actions, r );
				}
			}

			return r;
		}

		string GetParameters(Parameters p)
		{
			string parms = string.Empty;
			if( p.HasValues )
			{
				using( StringWriter sw = new StringWriter() )
				{
					Serializer serializer = new Serializer();
					serializer.Serialize( sw, p.Values );
					parms = sw.ToString();
				}
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