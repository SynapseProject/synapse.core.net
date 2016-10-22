using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public interface IHandlerRuntime
    {
        string ActionName { get; set; }

        IHandlerRuntime Initialize(string config);
        ExecuteResult Execute(string parms, HandlerStartInfo startInfo, bool dryRun = false); //maybe should be object

        event EventHandler<HandlerProgressCancelEventArgs> Progress;
    }

    public abstract class HandlerRuntimeBase : IHandlerRuntime
    {
        public string ActionName { get; set; }

        //public abstract string Parameters { get; set; }
        public abstract ExecuteResult Execute(string parms, HandlerStartInfo startInfo, bool dryRun = false);

        public virtual IHandlerRuntime Initialize(string config)
        {
            return this;
        }

        public event EventHandler<HandlerProgressCancelEventArgs> Progress;

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
    }

    //public interface IHandlerConfig
    //{
    //	string Key { get; set; }
    //}
}