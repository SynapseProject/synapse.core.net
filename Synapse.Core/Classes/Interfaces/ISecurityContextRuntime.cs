namespace Synapse.Core
{
    public interface ISecurityContextRuntime : IRuntimeComponent<ISecurityContextRuntime>, IRuntimeProvider
    {
        ExecuteResult Logon(SecurityContextStartInfo startInfo);
        void Logoff();
    }
}