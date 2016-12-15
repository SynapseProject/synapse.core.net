using System;
using System.Collections.Generic;

namespace Synapse.Core
{
    public interface IActionContainer
    {
        ActionItem ActionGroup { get; set; }
        List<ActionItem> Actions { get; set; }
        //bool IsAction { get; }

        ExecuteResult Result { get; set; }
    }
}