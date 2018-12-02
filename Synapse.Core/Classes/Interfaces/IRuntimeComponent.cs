using System;

namespace Synapse.Core
{
    public interface IRuntimeComponent
    {
        string ActionName { get; set; }
        string RuntimeType { get; set; }

        object GetConfigInstance();
        object GetParametersInstance();

        event EventHandler<LogMessageEventArgs> LogMessage;
    }

    public interface IRuntimeComponent<T> : IRuntimeComponent
    {
        T Initialize(string config);
    }

    public interface IRuntimeComponentCreator<T> where T : class, IRuntimeComponent<T>
    {
        T CreateRuntime(string planDefaultType, CryptoProvider planCrypto, string actionName);
    }
}