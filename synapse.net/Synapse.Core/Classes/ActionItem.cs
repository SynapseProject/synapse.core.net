using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public partial class ActionItem : IActionContainer, ICloneable<ActionItem>
    {
        public ActionItem()
        {
            Name = string.Empty;
            ExecuteCase = StatusType.None;
            Actions = new List<ActionItem>();
        }

        public string Name { get; set; }
        public string Proxy { get; set; }
        public StatusType ExecuteCase { get; set; }

        public HandlerInfo Handler { get; set; }

        public ExecuteResult Result { get; set; }
        [YamlIgnore]
        public bool HasResult { get { return Result != null; } }

        public ParameterInfo Parameters { get; set; }
        [YamlIgnore]
        public bool HasParameters { get { return Parameters != null; } }

        public ActionItem ActionGroup { get; set; }
        [YamlIgnore]
        public bool HasActionGroup { get { return ActionGroup != null; } }

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

        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public ActionItem Clone(bool shallow = true)
        {
            ActionItem a = new ActionItem()
            {
                Name = Name + $"_{DateTime.Now.Ticks}",
                Proxy = Proxy,
                ExecuteCase = ExecuteCase,
                Handler = Handler,
                Parameters = Parameters.Clone(),
                RunAs = RunAs,
                InstanceId = InstanceId
            };

            if( !shallow )
            {
                a.ActionGroup = ActionGroup;
                a.Actions = Actions;
            }

            return a;
        }

        public long PlanInstanceId { get; set; }

        public long InstanceId { get; set; }

        public override string ToString()
        {
            return string.Format( "{0}-->{1}", Name, Handler.Type );
        }
    }
}