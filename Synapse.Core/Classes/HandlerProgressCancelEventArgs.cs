using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Synapse.Core.Utilities;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class HandlerProgressCancelEventArgs : EventArgs, ICancelEventArgs
    {
        public HandlerProgressCancelEventArgs() { }

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
        [YamlIgnore()]
        public Exception Exception { get; protected set; }
        [YamlIgnore()]
        public bool HasException { get { return Exception != null; } }

        public ActionItem ToActionItem()
        {
            ExecuteResult result = new ExecuteResult()
            {
                Status = this.Status,
                Message = $"Context: {this.Context}, Message: {this.Message}",
                Sequence = this.Sequence,
            };
            return new ActionItem()
            {
                InstanceId = Id,
                Name = ActionName,
                Result = result,
            };
        }

        public string SerializeSimple(bool asYaml = false)
        {
            return asYaml ? ToYaml() : ToSingleLine();
        }

        public static HandlerProgressCancelEventArgs DeserializeSimple(string s, bool asYaml = false)
        {
            HandlerProgressCancelEventArgs hpcev = FromYaml( s );
            return hpcev;
        }


        public string ToSingleLine()
        {
            string exception = HasException ? $"|Exception: {Exception.Message}" : null;
            return $"ActionName: {ActionName}|Context: {Context}|Message: {Message}|Status: {Status}|Id: {Id}|Sequence: {Sequence}|Cancel: {Cancel}{exception}";
        }

        public string ToYaml()
        {
            return YamlHelpers.Serialize( this );
        }

        public static HandlerProgressCancelEventArgs FromYaml(string s)
        {
            return YamlHelpers.Deserialize<HandlerProgressCancelEventArgs>( s );
        }
    }
}