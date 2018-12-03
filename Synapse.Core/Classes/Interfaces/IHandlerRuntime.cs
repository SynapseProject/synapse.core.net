using System;

namespace Synapse.Core
{
    //Handlers are _not_ IRuntimeProvider
    public interface IHandlerRuntime : IRuntimeComponent<IHandlerRuntime>
    {
        ExecuteResult Execute(HandlerStartInfo startInfo);

        event EventHandler<HandlerProgressCancelEventArgs> Progress;
    }
}