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

        public void EnsureInitialized()
        {
            if( Handler == null )
                Handler = new HandlerInfo();

            if( Result == null )
                Result = new ExecuteResult();
        }

        public string Name { get; set; }
        public string Description { get; set; }

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

        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public ActionItem Clone(bool shallow = true)
        {
            EnsureInitialized();

            ActionItem a = new ActionItem()
            {
                Name = Name,    // + $"_{DateTime.Now.Ticks}",
                Description = Description,
                Proxy = Proxy,
                ExecuteCase = ExecuteCase,
                Handler = Handler.Clone(),
                Parameters = HasParameters ? Parameters.Clone() : null,
                RunAs = RunAs,
                InstanceId = InstanceId
            };

            if( !shallow )
            {
                a.ActionGroup = ActionGroup;
                a.Actions = Actions;
            }

            a.EnsureInitialized();

            return a;
        }

        public long PlanInstanceId { get; set; }

        public long InstanceId { get; set; }

        public override string ToString()
        {
            return string.Format( "Name: {0}-->Handler: {1}", Name, Handler?.ToString() ?? "Unspecified" );
        }


        public object _id { get; set; }
        public long ParentInstanceId { get; set; }



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
    }
}