﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Synapse.Core.Runtime
{
    public interface IPlanRuntimeContainer
    {
        Plan Plan { get; }
        bool IsDryRun { get; }
        Dictionary<string, string> DynamicData { get; }
        long PlanInstanceId { get; }

        void Start(CancellationToken token, Action<IPlanRuntimeContainer> callback);
    }
}