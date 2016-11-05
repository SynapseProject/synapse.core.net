﻿using System;
using System.IO;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class LogMessageEventArgs : EventArgs
    {
        public LogMessageEventArgs() { }

        public LogMessageEventArgs(string context, string message, LogLevel level = LogLevel.Info, Exception ex = null)
        {
            Context = context;
            Message = message;
            Exception = ex;
            Level = HasException ? LogLevel.Fatal : level;
        }

        public string ActionName { get; internal set; }
        public string Context { get; protected set; }
        public string Message { get; protected set; }
        public LogLevel Level { get; protected set; }

        [XmlIgnore]
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
                str = sw.ToString(); //.Replace( "\r\n", "|" );
            }
            return str; //.TrimEnd( '|' );

            //return ToXml();
        }

        public static LogMessageEventArgs DeserializeSimple(string s)
        {
            LogMessageEventArgs args = null;
            //s = s.Replace( "|", "\r\n" );
            using( StringReader sr = new StringReader( s ) )
                args = FromYaml( sr );
            return args;
        }

        public string ToXml(bool indented = false)
        {
            return Utilities.XmlHelpers.Serialize<LogMessageEventArgs>( this, indented: indented );
        }

        public static LogMessageEventArgs FromXml(TextReader reader)
        {
            return Utilities.XmlHelpers.Deserialize<LogMessageEventArgs>( reader );
        }

        public void ToYaml(TextWriter tw)
        {
            Serializer serializer = new Serializer();
            serializer.Serialize( tw, this );
        }

        public static LogMessageEventArgs FromYaml(TextReader reader)
        {
            Deserializer deserializer = new Deserializer( ignoreUnmatched: false );
            return deserializer.Deserialize<LogMessageEventArgs>( reader );
        }
    }
}