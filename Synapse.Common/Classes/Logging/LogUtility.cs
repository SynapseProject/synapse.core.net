using System;
using System.IO;
using log4net;
using log4net.Appender.Dynamic;

namespace Synapse.Common
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class LogUtility : IDisposable
    {
        bool _disposed = false;

        //public static readonly ILog Default = log4net.LogManager.GetLogger( "SynapseNodeServer" );

        public ILog _logger = null;

        public LogUtility()
        {
        }
        ~LogUtility()
        {
            Dispose();
        }

        public void InitDefaultLogger(string loggerName, string appenderName,
            string logfileName, string conversionPattern, string levelName = "ALL")
        {
            //_logger = log4net.LogManager.GetLogger( "SynapseNodeServer" );
            _logger = new RollingFileAppenderHelper( loggerName, appenderName, logfileName, conversionPattern, levelName ).Log;
        }

        public void InitDynamicFileAppender(string loggerName, string appenderName,
            string logfileName, string conversionPattern, string levelName = "ALL")
        {
            _logger = new DynamicFileAppender( loggerName, appenderName, logfileName, conversionPattern, levelName ).Log;
        }

        #region Write/WriteFormat
        public void WriteFormat(string format, params object[] args)
        {
            Write( LogLevel.Info, string.Format( format, args ) );
        }

        public void WriteFormat(LogLevel level, string format, params object[] args)
        {
            Write( level, string.Format( format, args ) );
        }

        public void Write(object message)
        {
            Write( LogLevel.Info, message );
        }

        public void Write(LogLevel level, object message, Exception ex = null, ILog logger = null)
        {
            if( logger == null )
            {
                if( _logger != null )
                    logger = _logger;
                //else
                //    logger = Default;
            }

            if( ex != null && (level == LogLevel.Debug || level == LogLevel.Info) )
                level = LogLevel.Error;

            switch( level )
            {
                case LogLevel.Debug: logger.Debug( message );
                break;

                case LogLevel.Error: logger.Error( message, ex );
                break;

                case LogLevel.Fatal: logger.Fatal( message, ex );
                break;

                case LogLevel.Info: logger.Info( message );
                break;

                case LogLevel.Warn: logger.Warn( message );
                break;
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if( !_disposed )
            {
                LogManager.Shutdown();
            }
            _disposed = true;

            GC.SuppressFinalize( this );
        }
        #endregion
    }
}
