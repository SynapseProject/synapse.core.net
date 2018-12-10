using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Synapse.Core
{
    public interface IActionContainer
    {
        string Name { get; set; }
        string Description { get; set; }

        SecurityContext RunAs { get; set; }

        ActionItem ActionGroup { get; set; }
        List<ActionItem> Actions { get; set; }
        ConcurrentBag<ActionItem> ActionsBag { get; set; }
        //bool IsAction { get; }

        ExecuteResult Result { get; set; }

        void EnsureInitialized();
    }
}