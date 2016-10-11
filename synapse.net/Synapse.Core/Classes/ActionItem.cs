using System;
using System.Collections.Generic;
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
        public string Proxy { get; set; }
        public StatusType ExecuteCase { get; set; }
        public HandlerInfo Handler { get; set; }
        public ParameterInfo Parameters { get; set; }
        [YamlIgnore]
        public bool HasParameters { get { return Parameters != null; } }
        public HandlerResult ExecuteResult { get; set; }
        public ActionItem ActionGroup { get; set; }
        public List<ActionItem> Actions { get; set; }
        [YamlIgnore]
        public bool HasActions { get { return ActionGroup != null || (Actions != null && Actions.Count > 0); } }
        public SecurityContext RunAs { get; set; }
        [YamlIgnore]
        public bool HasRunAs { get { return RunAs != null; } }

        public static ActionItem CreateDummy(string name = "xxx")
        {
            return new ActionItem()
            {
                Name = name,
                Handler = new HandlerInfo() { Type = "foo" },
                Parameters = new ParameterInfo() { Values = "foo" },
                Actions = new List<ActionItem>()
            };
        }

        public ActionItem Clone()
        {
            ActionItem a = new ActionItem()
            {
                Name = Name,
                Proxy = Proxy,
                ExecuteCase = ExecuteCase,
                Handler = Handler,
                Parameters = Parameters,
                RunAs = RunAs
            };

            return a;
        }

        public override string ToString()
        {
            return string.Format( "{0}-->{1}", Name, Handler.Type );
        }
    }
}