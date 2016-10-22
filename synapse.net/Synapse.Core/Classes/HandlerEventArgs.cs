using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public interface ICancelEventArgs
    {
        bool Cancel { get; set; }
    }
    public class HandlerProgressCancelEventArgs : EventArgs, ICancelEventArgs
    {
        public HandlerProgressCancelEventArgs(string context, string message,
            StatusType status = StatusType.Running, long id = 0, int sequence = 0, bool cancel = false, Exception ex = null)
        {
            Context = context;
            Message = message;
            Status = status;
            Id = id;
            Sequence = sequence;
            Exception = ex;
        }

        public string ActionName { get; internal set; }
        public string Context { get; protected set; }
        public string Message { get; protected set; }
        public StatusType Status { get; protected set; }
        public long Id { get; protected set; }
        public int Sequence { get; protected set; }
        public bool Cancel { get; set; }
        public Exception Exception { get; protected set; }
        public bool HasException { get { return Exception != null; } }
    }
}