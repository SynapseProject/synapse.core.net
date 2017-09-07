using System;
using System.Collections.Generic;
using System.Threading;

namespace Synapse.Core
{
    public interface IInheritable
    {
        bool Inheritable { get; set; }
        bool AllowInheritance { get; set; }
    }
}