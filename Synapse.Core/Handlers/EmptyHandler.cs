﻿using System;

using Synapse.Core;

namespace Synapse.Handlers
{
    public class EmptyHandler : HandlerRuntimeBase
    {
        override public ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            EmptyHandlerParameters parms = DeserializeOrNew<EmptyHandlerParameters>( startInfo.Parameters );

            OnProgress( "Execute", parms.ReturnStatus.ToString(), parms.ReturnStatus, startInfo.InstanceId, Int32.MaxValue );

            return new ExecuteResult() { Status = parms.ReturnStatus, ExitData = parms.ExitData };
        }
    }

    public class EmptyHandlerParameters
    {
        public EmptyHandlerParameters()
        {
            ReturnStatus = StatusType.None;
            ExitData = "EmptyHandler ExitData default value.";
        }

        public StatusType ReturnStatus { get; set; }
        public string ExitData { get; set; }
    }
}