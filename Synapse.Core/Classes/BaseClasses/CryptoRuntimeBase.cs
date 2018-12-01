using System;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public abstract class CryptoRuntimeBase : ICryptoRuntime
    {
        public string ActionName { get; set; }
        public string RuntimeType { get; set; }

        public abstract object GetConfigInstance();
        public abstract object GetParametersInstance();


        public abstract ExecuteResult Encrypt(CryptoStartInfo startInfo);
        public abstract ExecuteResult Decrypt(CryptoStartInfo startInfo);


        public virtual ICryptoRuntime Initialize(string config)
        {
            return this;
        }

        /// <summary>
        /// Tries to desrialize parameters first as YAML/JSON, then tries XML.  Returns deserialized class or new T().
        /// </summary>
        /// <typeparam name="T">The class type for deserialization.</typeparam>
        /// <param name="parameters">The string to deserialize.</param>
        /// <returns>Deserialized class or new T().</returns>
        public virtual T DeserializeOrNew<T>(string parameters) where T : class, new()
        {
            T parms = null;

            try
            {
                parms = YamlHelpers.Deserialize<T>( parameters );
            }
            catch
            {
                try
                {
                    parms = XmlHelpers.Deserialize<T>( parameters );
                }
                catch
                {
                    parms = new T();
                }
            }

            return parms;
        }

        /// <summary>
        /// Tries to desrialize parameters first as YAML/JSON, then tries XML.  Returns deserialized class or default( T ).
        /// </summary>
        /// <typeparam name="T">The class type for deserialization.</typeparam>
        /// <param name="parameters">The string to deserialize.</param>
        /// <returns>Deserialized class or default( T ).</returns>
        public virtual T DeserializeOrDefault<T>(string parameters) where T : class
        {
            T parms = null;

            try
            {
                parms = YamlHelpers.Deserialize<T>( parameters );
            }
            catch
            {
                try
                {
                    parms = XmlHelpers.Deserialize<T>( parameters );
                }
                catch
                {
                    parms = default( T );
                }
            }

            return parms;
        }

        public event EventHandler<LogMessageEventArgs> LogMessage;

        protected virtual void OnLogMessage(string context, string message, LogLevel level = LogLevel.Info, Exception ex = null)
        {
            LogMessageEventArgs args = new LogMessageEventArgs( context, message, level, ex );
            LogMessage?.Invoke( this, args );
        }
    }
}