using System;

namespace Synapse.Core
{
    public interface IReplacementValueOptions
    {
        string Replace { get; set; }
        string Encode { get; set; }
    }
}