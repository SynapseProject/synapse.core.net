using System;

namespace Synapse.Service.Common
{
    public enum ServiceStatus
    {
        Starting,
        Running,
        Pausing,
        Paused,
        Resuming,
        Stopping,
        Stopped,
        Disabled,
        Recovering,
        DrainStopping
    };
}