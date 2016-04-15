using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core
{
	public class ActionItem
	{
		public ActionItem()
		{
			Name = string.Empty;
			//GroupName = string.Empty;
			ResultCase = HandlerResult.DefaultExitCode;
		}

		public string Name { get; set; }
		//public string GroupName { get; set; }
		public int ResultCase { get; set; }
		public HandlerInfo Handler { get; set; }
		public Parameters Parameters { get; set; }
		public List<ActionItem> Actions { get; set; }
		public bool HasActions { get { return Actions != null && Actions.Count > 0; } }

		public static ActionItem CreateDummy(string name = "xxx")
		{
			return new ActionItem()
			{
				Name = name,
				//GroupName = "meow",
				Handler = new HandlerInfo() { Type = "foo", ConfigKey = "zzz" },
				Parameters = new Parameters() { Values = "foo" },
				Actions = new List<ActionItem>()
			};
		}

		public override string ToString()
		{
			return string.Format( "{0}-->{1}", Name, Handler.Type );
		}
	}
}