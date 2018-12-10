﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Synapse.Core.Utilities;

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

            if( HasRunAs )
                RunAs.EnsureInitialized();

            if( Result == null )
                Result = new ExecuteResult();

            if( _actionsBag == null )
                _actionsBag = Actions != null ? Actions.ToConcurrentBag() : new ConcurrentBag<ActionItem>();
        }

        public string Name { get; set; }
        public string Description { get; set; }

        public string Proxy { get; set; }
        public StatusType ExecuteCase { get; set; }

        public SaveExitDataInfo SaveExitDataAs { get; set; }
        [YamlIgnore]
        public bool HasSaveExitDataAs { get { return SaveExitDataAs != null; } }

        public HandlerInfo Handler { get; set; }

        public SecurityContext RunAs { get; set; }
        [YamlIgnore]
        public bool HasRunAs { get { return RunAs != null; } }
        //[YamlIgnore]
        //public bool HasValidRunAs { get { return RunAs != null && RunAs.IsValid; } }

        public ParameterInfo Parameters { get; set; }
        [YamlIgnore]
        public bool HasParameters { get { return Parameters != null; } }

        public ActionItem ActionGroup { get; set; }
        [YamlIgnore]
        public bool HasActionGroup { get { return ActionGroup != null; } }

        public List<ActionItem> Actions { get; set; }
        [YamlIgnore]
        public bool HasActions { get { return ActionGroup != null || (Actions != null && Actions.Count > 0); } }

        ConcurrentBag<ActionItem> _actionsBag = null;
        [YamlIgnore]
        public ConcurrentBag<ActionItem> ActionsBag
        {
            get
            {
                if( _actionsBag == null )
                    _actionsBag = Actions?.ToConcurrentBag();

                return _actionsBag;
            }
            set { _actionsBag = value; }
        }


        public ExecuteResult Result { get; set; }
        [YamlIgnore]
        public bool HasResult { get { return Result != null; } }


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
                SaveExitDataAs = HasSaveExitDataAs ? SaveExitDataAs.Clone() : null,
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

        //24 Mar, 2017: not in use
        //[YamlIgnore]
        //public long PlanInstanceId { get; set; }

        public long InstanceId { get; set; }

        public override string ToString()
        {
            return string.Format( "Name: {0}-->Handler: {1}", Name, Handler?.ToString() ?? "Unspecified" );
        }


        //24 Mar, 2017: not in use
        //[YamlIgnore]
        //public object _id { get; set; }
        [YamlIgnore]
        public long ParentInstanceId { get; set; }



        public static ActionItem CreateSample(string name = null)
        {
            return new ActionItem()
            {
                Name = string.IsNullOrWhiteSpace( name ) ? "Sample Action" : name,
                Description = "Sample Action friendly description.",
                ExecuteCase = StatusType.Success | StatusType.Failed | StatusType.Tombstoned,
                Proxy = "Future-use: http://host:port/synapse/node",
                SaveExitDataAs = SaveExitDataInfo.CreateSample(),
                Handler = HandlerInfo.CreateSample(),
                Parameters = ParameterInfo.CreateSample(),
                RunAs = SecurityContext.CreateSample(),
                Result = ExecuteResult.CreateSample(),

                ActionGroup = new ActionItem()
                {
                    Name = string.IsNullOrWhiteSpace( name ) ? "Sample ActionGroup" : name,
                    Description = "Sample Action friendly description.",
                    ExecuteCase = StatusType.Success | StatusType.Failed | StatusType.Tombstoned,
                    Proxy = "Future-use: http://host:port/synapse/node",
                    SaveExitDataAs = SaveExitDataInfo.CreateSample(),
                    Handler = HandlerInfo.CreateSample(),
                    Parameters = ParameterInfo.CreateSample(),
                    RunAs = SecurityContext.CreateSample(),
                    Result = ExecuteResult.CreateSample()
                }
            };
        }
    }
}