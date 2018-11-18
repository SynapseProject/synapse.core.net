using System;

namespace Synapse.Core
{
    public interface ISecurityContextRuntime
    {
        string ActionName { get; set; }
        string RuntimeType { get; set; }

        object GetConfigInstance();
        object GetParametersInstance();

        ISecurityContextRuntime Initialize(string config);

        ExecuteResult Logon(SecurityContextStartInfo startInfo);
        void Logoff();

        event EventHandler<LogMessageEventArgs> LogMessage;
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