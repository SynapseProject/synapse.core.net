using System;

namespace Synapse.Core
{
    public interface ISecurityContextRuntime : IRuntimeComponent<ISecurityContextRuntime>
    {
        ExecuteResult Logon(SecurityContextStartInfo startInfo);
        void Logoff();
    }

    public class SecurityContextStartInfo : StartInfoBase
    {
        public SecurityContextStartInfo() { }

        /// <summary>
        /// Initialize this instance from the parameter instance.
        /// </summary>
        /// <param name="si">StartInfo instance used to initialise this instance.</param>
        public SecurityContextStartInfo(IStartInfo si)
        {
            RequestNumber = si.RequestNumber;
            RequestUser = si.RequestUser;
        }

        public bool IsDryRun { get; internal set; }

        public string Parameters { get; internal set; }

        public CryptoProvider Crypto { get; internal set; }
    }
}