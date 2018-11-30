using System;
using System.IO;
using Synapse.Core.Utilities;
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

        [YamlIgnore()]
        public Exception Exception { get; protected set; }
        [YamlIgnore()]
        public bool HasException { get { return Exception != null; } }


        public string SerializeSimple(bool asYaml = false)
        {
            return asYaml ? ToYaml() : ToSingleLine();
        }

        public static LogMessageEventArgs DeserializeSimple(string s, bool asYaml = false)
        {
            LogMessageEventArgs args = FromYaml( s );
            return args;
        }

        public string ToSingleLine()
        {
            string exception = HasException ? $"|Exception: {Exception.Message}" : null;
            return $"ActionName: {ActionName}|Context: {Context}|Message: {Message}|LogLevel: {Level}{exception}";
        }

        public string ToYaml()
        {
            return YamlHelpers.Serialize( this );
        }

        public static LogMessageEventArgs FromYaml(string s)
        {
            return YamlHelpers.Deserialize<LogMessageEventArgs>( s );
        }
    }
}