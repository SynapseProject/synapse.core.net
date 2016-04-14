using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core
{
	public class Plan
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public List<ActionItem> Actions { get; set; }
		public bool IsActive { get; set; }
	}

	public class ActionItem
	{
		public string Name { get; set; }
		public string GroupName { get; set; }
		public int ResultCase { get; set; }
		public HandlerInfo Handler { get; set; }
		public Parameters Parameters { get; set; }
		public List<ActionItem> Actions { get; set; }
		public bool HasActions { get { return Actions != null && Actions.Count > 0; } }

		public static ActionItem Create(string name = "xxx")
		{
			return new ActionItem()
			{
				Name = name,
				GroupName = "meow",
				Handler = new HandlerInfo() { Type = "yyy", ConfigKey = "zzz" },
				Parameters = new Parameters() { Values = "foo" },
				Actions = new List<ActionItem>()
			};
		}
	}

	public class Parameters
	{
		public string Uri { get; set; }
		public object Values { get; set; }
	}
}