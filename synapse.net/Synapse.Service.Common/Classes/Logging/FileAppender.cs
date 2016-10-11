using System;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace log4net.Appender.Dynamic
{
    //note: code adapted from:
    //http://stackoverflow.com/questions/308436/log4net-programmatically-specify-multiple-loggers-with-multiple-file-appenders
    public class FileAppenderFactory
    {
        public static IAppender CreateFileAppender(string appenderName, string logfileName, string conversionPattern)
        {
            FileAppender appender = new FileAppender();
            appender.Name = appenderName;
            appender.File = logfileName;
            appender.AppendToFile = true;
            appender.LockingModel = new FileAppender.MinimalLock();

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = conversionPattern;
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            return appender;
        }

        public static void AddAppender(string loggerName, string levelName, IAppender appender)
        {
            ILog log = LogManager.GetLogger( loggerName );
            Logger l = (Logger)log.Logger;

            l.Level = l.Hierarchy.LevelMap[levelName];
            l.AddAppender( appender );
            l.Repository.Configured = true; //critical line
        }

        public static void RemoveAppender(string loggerName, IAppender appender)
        {
            ILog log = LogManager.GetLogger( loggerName );
            Logger l = (Logger)log.Logger;

            l.RemoveAppender( appender );
            l.Repository.Configured = true; //critical line
        }
    }

    public class DynamicFileAppender : IDisposable
    {
        string _loggerName = string.Empty;
        IAppender _appender = null;
        ILog _log = null;

        public DynamicFileAppender(string loggerName, string appenderName,
            string logfileName, string conversionPattern, string levelName = "ALL")
        {
            _loggerName = loggerName;
            _appender = FileAppenderFactory.CreateFileAppender( appenderName, logfileName, conversionPattern );
            FileAppenderFactory.AddAppender( loggerName, levelName, _appender );
            _log = LogManager.GetLogger( loggerName );
        }

        public ILog Log { get { return _log; } }

        public void Dispose()
        {
            FileAppenderFactory.RemoveAppender( _loggerName, _appender );
            _log = null;
        }
    }
}