using System;

namespace Synapse.Core
{
    public interface IHandlerRuntime : IRuntimeComponent<IHandlerRuntime>
    {
        ExecuteResult Execute(HandlerStartInfo startInfo);

        event EventHandler<HandlerProgressCancelEventArgs> Progress;
    }
}