using System;
using System.Collections.Generic;
using System.Threading;

namespace Synapse.Core
{
    public interface IInheritable
    {
        bool IsInheritable { get; set; }
        bool BlockInheritance { get; set; }
    }
}