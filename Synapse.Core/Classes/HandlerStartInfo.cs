using System;
using System.Collections.Generic;

namespace Synapse.Core
{
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

        public long PlanInstanceId { get; internal set; }

        public long InstanceId { get; internal set; }

        public bool IsDryRun { get; internal set; }

        public string Parameters { get; internal set; }

        public object ParentExitData { get; internal set; }

        public SecurityContext RunAs { get; internal set; }

        public CryptoProvider Crypto { get; internal set; }
    }

    /// <summary>
    /// this is a convenience-wrapper on HandlerStartInfo that is used to avoid
    /// serialization of props not mentioned here (mainly RunAs and Crypto)
    /// </summary>
    public class HandlerStartInfoData
    {
        public HandlerStartInfoData()
        { }

        public HandlerStartInfoData(HandlerStartInfo handlerStartInfo)
        {
            Parameters = handlerStartInfo.Parameters;
            ParentExitData = handlerStartInfo.ParentExitData;
        }

        public string Parameters { get; set; }

        public object ParentExitData { get; set; }
    }
}