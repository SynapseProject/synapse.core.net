using System.Collections.Generic;

namespace Synapse.Core.Runtime
{
    public interface IPlanRuntimeContainer
    {
        Dictionary<string, string> DynamicData { get; }
        bool IsDryRun { get; }
        Plan Plan { get; }

        void Start();
    }
}