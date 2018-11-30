using System;

namespace Synapse.Core
{
    public interface ISecurityContextRuntime : IRuntimeComponent<ISecurityContextRuntime>
    {
        ExecuteResult Logon(SecurityContextStartInfo startInfo);
        void Logoff();
    }
}