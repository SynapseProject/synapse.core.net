namespace Synapse.Core
{
    public interface ICryptoRuntime : IRuntimeComponent<ICryptoRuntime>, IRuntimeProvider
    {
        ExecuteResult Encrypt(CryptoStartInfo startInfo);
        ExecuteResult Decrypt(CryptoStartInfo startInfo);
    }
}