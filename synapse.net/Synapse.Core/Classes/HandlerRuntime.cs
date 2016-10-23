using System;

namespace Synapse.Core
{
    public interface IHandlerRuntime
    {
        string ActionName { get; set; }

        IHandlerRuntime Initialize(string config);
        ExecuteResult Execute(HandlerStartInfo startInfo);

        event EventHandler<HandlerProgressCancelEventArgs> Progress;
        event EventHandler<LogMessageEventArgs> LogMessage;
    }

    public abstract class HandlerRuntimeBase : IHandlerRuntime
    {
        public string ActionName { get; set; }

        //public abstract string Parameters { get; set; }
        public abstract ExecuteResult Execute(HandlerStartInfo startInfo);

        public virtual IHandlerRuntime Initialize(string config)
        {
            return this;
        }

        public event EventHandler<HandlerProgressCancelEventArgs> Progress;
        public event EventHandler<LogMessageEventArgs> LogMessage;

        /// <summary>
        /// Notify of step progress.
        /// </summary>
        /// <param name="context">The method name or workflow activty.</param>
        /// <param name="message">Descriptive message.</param>
        /// <param name="status">Overall Package status indicator.</param>
        /// <param name="id">Message Id.</param>
        /// <param name="sequence">Message/error severity.</param>
        /// <param name="ex">Current exception (optional).</param>
        protected virtual bool OnProgress(string context, string message,
            StatusType status = StatusType.Running, long id = 0, int sequence = 0, bool cancel = false, Exception ex = null)
        {
            HandlerProgressCancelEventArgs e =
                new HandlerProgressCancelEventArgs( context, message, status, id, sequence, cancel, ex ) { ActionName = this.ActionName };

            Progress?.Invoke( this, e );

            return e.Cancel;
        }

        protected virtual void OnLogMessage(string context, string message, LogLevel level = LogLevel.Info, Exception ex = null)
        {
            LogMessageEventArgs args = new LogMessageEventArgs( context, message, level, ex );
            LogMessage?.Invoke( this, args );
        }
    }
}