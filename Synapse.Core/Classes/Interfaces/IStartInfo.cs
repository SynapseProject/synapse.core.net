using System;

namespace Synapse.Core
{
    public interface IStartInfo
    {
        string RequestUser { get; set; }
        string RequestNumber { get; set; }
    }
}