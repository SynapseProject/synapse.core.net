using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
	public class ActionItem
	{
		public ActionItem()
		{
			Name = string.Empty;
			ExecuteCase = StatusType.None;
		}

		public string Name { get; set; }
		public StatusType ExecuteCase { get; set; }
		public HandlerInfo Handler { get; set; }
		public Parameters Parameters { get; set; }
		[YamlIgnore]
		public bool HasParameters { get { return Parameters != null; } }
		public HandlerResult ExecuteResult { get; set; }
		public List<ActionItem> Actions { get; set; }
		[YamlIgnore]
		public bool HasActions { get { return Actions != null && Actions.Count > 0; } }

		public static ActionItem CreateDummy(string name = "xxx")
		{
			return new ActionItem()
			{
				Name = name,
				Handler = new HandlerInfo() { Type = "foo" },
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