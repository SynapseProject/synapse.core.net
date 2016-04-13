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
		public string GroupKey { get; set; }
		public string ResultCase { get; set; }
		public HandlerRef Handler { get; set; }
		public Parameters Parameters { get; set; }
		public List<ActionItem> Actions { get; set; }

		public static ActionItem Create(string name = "xxx")
		{
			return new ActionItem()
			{
				Name = name,
				GroupKey = "meow",
				Handler = new HandlerRef() { Type = "yyy", ConfigKey = "zzz" },
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

	public class HandlerRef
	{
		public string Type { get; set; }
		public string ConfigKey { get; set; }
		public string ConfigValues { get; set; }
	}

	public class HandlerLibrary
	{
		public string Name { get; set; }
		public List<IHandlerConfig> Configs { get; set; }
	}

	public interface IHandlerConfig
	{
		string Key { get; set; }
	}
}