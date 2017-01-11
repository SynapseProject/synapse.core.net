using System;

namespace Synapse.Services.Common
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