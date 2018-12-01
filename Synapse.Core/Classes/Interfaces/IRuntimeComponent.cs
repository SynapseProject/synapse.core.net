using System;

namespace Synapse.Core
{
    public interface IRuntimeComponent<T>
    {
        string ActionName { get; set; }
        string RuntimeType { get; set; }

        object GetConfigInstance();
        object GetParametersInstance();

        T Initialize(string config);

        event EventHandler<LogMessageEventArgs> LogMessage;
    }

    public interface IRuntimeComponentCreator<T> where T : class, IRuntimeComponent<T>
    {
        T CreateRuntime(string planDefaultType, CryptoProvider planCrypto, string actionName);
    }
}