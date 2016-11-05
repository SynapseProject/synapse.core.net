using System;

namespace Synapse.Core
{
    public abstract class StartInfoBase : IStartInfo
    {
        public string RequestUser { get; set; }
        public string RequestNumber { get; set; }
    }

    public class PlanStartInfo : StartInfoBase
    {
    }

    public class HandlerStartInfo : StartInfoBase
    {
        public HandlerStartInfo() { }

        /// <summary>
        /// Initialize this instance from the parameter instance.
        /// </summary>
        /// <param name="si">StartInfo instance used to initialise this instance.</param>
        public HandlerStartInfo(IStartInfo si)
        {
            RequestNumber = si.RequestNumber;
            RequestUser = si.RequestUser;
        }

        public long InstanceId { get; internal set; }

        public bool IsDryRun { get; internal set; }

        public string Parameters { get; internal set; }
    }
}