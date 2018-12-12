using System;

namespace Synapse.Core
{
    public interface ISecurityContextRuntime : IRuntimeComponent<ISecurityContextRuntime>, IRuntimeProvider
    {
        ExecuteResult Logon(SecurityContextStartInfo startInfo);
        void Logoff();
        bool UseExecuteProxy { get; set; }
        ExecuteResult ExecuteProxy(Func<HandlerStartInfo, ExecuteResult> func, HandlerStartInfo handlerStartInfo);
    }
}