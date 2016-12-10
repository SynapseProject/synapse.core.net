using System;

namespace Synapse.Core
{
    public interface ICancelEventArgs
    {
        bool Cancel { get; set; }
    }
}