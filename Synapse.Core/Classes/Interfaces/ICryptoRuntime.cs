namespace Synapse.Core
{
    public interface ICryptoRuntime : IRuntimeComponent<ICryptoRuntime>
    {
        ExecuteResult Encrypt(CryptoStartInfo startInfo);
        ExecuteResult Decrypt(CryptoStartInfo startInfo);
    }
}