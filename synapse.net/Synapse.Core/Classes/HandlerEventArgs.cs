using System;
using System.Collections.Generic;
using System.IO;
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

        public string SerializeSimple()
        {
            //return $"ActionName: {ActionName}|Context: {Context}|Message: {Message}|Status: {Status}|Id: {Id}|Sequence: {Sequence}|Cancel: {Cancel}";
            string str = null;
            using( StringWriter sw = new StringWriter() )
            {
                ToYaml( sw );
                str = sw.ToString().Replace( "\r\n", "|" );
            }
            return str.TrimEnd( '|' );
        }

        public static HandlerProgressCancelEventArgs DeserializeSimple(string s)
        {
            HandlerProgressCancelEventArgs hpcev = null;
            s = s.Replace( "|", "\r\n" );
            using( StringReader sr = new StringReader( s ) )
                hpcev = FromYaml( sr );
            return hpcev;
        }


        public void ToYaml(TextWriter tw)
        {
            Serializer serializer = new Serializer();
            serializer.Serialize( tw, this );
        }

        public static HandlerProgressCancelEventArgs FromYaml(TextReader reader)
        {
            Deserializer deserializer = new Deserializer( ignoreUnmatched: false );
            return deserializer.Deserialize<HandlerProgressCancelEventArgs>( reader );
        }

    }
}