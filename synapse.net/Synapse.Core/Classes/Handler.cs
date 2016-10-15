using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class HandlerInfo
    {
        public string Type { get; set; }
        public ParameterInfo Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return Config != null; } }
    }

    public interface IHandlerRuntime
    {
        string ActionName { get; set; }

        IHandlerRuntime Initialize(string config);
        ExecuteResult Execute(string parms, bool dryRun = false); //maybe should be object

        event EventHandler<HandlerProgressCancelEventArgs> Progress;
    }

    public abstract class HandlerRuntimeBase : IHandlerRuntime
    {
        public string ActionName { get; set; }

        //public abstract string Parameters { get; set; }
        public abstract ExecuteResult Execute(string parms, bool dryRun = false);

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
        /// <param name="severity">Message/error severity.</param>
        /// <param name="ex">Current exception (optional).</param>
        protected virtual bool OnProgress(string context, string message,
            StatusType status = StatusType.Running, int id = 0, int severity = 0, bool cancel = false, Exception ex = null)
        {
            HandlerProgressCancelEventArgs e =
                new HandlerProgressCancelEventArgs( context, message, status, id, severity, cancel, ex ) { ActionName = this.ActionName };

            Progress?.Invoke( this, e );

            return e.Cancel;
        }
    }

    public interface ICancelEventArgs
    {
        bool Cancel { get; set; }
    }
    public class HandlerProgressCancelEventArgs : EventArgs, ICancelEventArgs
    {
        public HandlerProgressCancelEventArgs(string context, string message,
            StatusType status = StatusType.Running, int id = 0, int severity = 0, bool cancel = false, Exception ex = null)
        {
            Context = context;
            Message = message;
            Status = status;
            Id = id;
            Severity = severity;
            Exception = ex;
        }

        public string ActionName { get; internal set; }
        public string Context { get; protected set; }
        public string Message { get; protected set; }
        public StatusType Status { get; protected set; }
        public int Id { get; protected set; }
        public int Severity { get; protected set; }
        public bool Cancel { get; set; }
        public Exception Exception { get; protected set; }
        public bool HasException { get { return Exception != null; } }
    }

    //public interface IHandlerConfig
    //{
    //	string Key { get; set; }
    //}
}