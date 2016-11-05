using System;

namespace Synapse.Core
{
    public abstract class StartInfoBase : IStartInfo
    {
        public string RequestUser { get; set; }
        public string RequestNumber { get; set; }
    }
}