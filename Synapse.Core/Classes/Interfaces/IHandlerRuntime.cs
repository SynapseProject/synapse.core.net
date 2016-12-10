using System;

namespace Synapse.Core
{
    public interface IHandlerRuntime
    {
        string ActionName { get; set; }

        string RuntimeType { get; set; }

        IHandlerRuntime Initialize(string config);
        ExecuteResult Execute(HandlerStartInfo startInfo);

        event EventHandler<HandlerProgressCancelEventArgs> Progress;
        event EventHandler<LogMessageEventArgs> LogMessage;
    }
}