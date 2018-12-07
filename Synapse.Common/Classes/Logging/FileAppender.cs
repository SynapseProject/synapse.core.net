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
            PatternLayout layout = new PatternLayout
            {
                ConversionPattern = conversionPattern
            };
            layout.ActivateOptions();

            FileAppender appender = new FileAppender
            {
                Name = appenderName,
                File = logfileName,
                AppendToFile = true,
                LockingModel = new FileAppender.MinimalLock(),
                Layout = layout
            };
            appender.ActivateOptions();

            return appender;
        }

        public static IAppender CreateRollingFileAppender(string appenderName, string logfileName, string conversionPattern)
        {
            PatternLayout layout = new PatternLayout
            {
                ConversionPattern = conversionPattern
            };
            layout.ActivateOptions();

            RollingFileAppender appender = new RollingFileAppender
            {
                Name = appenderName,
                File = logfileName,
                StaticLogFileName = true,
                AppendToFile = false,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = 10,
                MaximumFileSize = "10MB",
                PreserveLogFileNameExtension = true,
                Layout = layout
            };
            appender.ActivateOptions();

#if NET45
            Config.BasicConfigurator.Configure( appender );
#else
            Repository.ILoggerRepository repo = LogManager.GetRepository( System.Reflection.Assembly.GetCallingAssembly() );
            Config.BasicConfigurator.Configure( repo, appender );
#endif

            return appender;
        }

        public static void AddAppender(string loggerName, string levelName, IAppender appender)
        {
#if NET45
            ILog log = LogManager.GetLogger( loggerName );
#else
            Repository.ILoggerRepository repo = LogManager.GetRepository( System.Reflection.Assembly.GetCallingAssembly() );
            ILog log = LogManager.GetLogger( repo.Name, loggerName );
#endif
            Logger l = (Logger)log.Logger;

            l.Level = l.Hierarchy.LevelMap[levelName];
            l.AddAppender( appender );
            l.Repository.Configured = true; //critical line
        }

        public static void RemoveAppender(string loggerName, IAppender appender)
        {
#if NET45
            ILog log = LogManager.GetLogger( loggerName );
#else
            Repository.ILoggerRepository repo = LogManager.GetRepository( System.Reflection.Assembly.GetCallingAssembly() );
            ILog log = LogManager.GetLogger( repo.Name, loggerName );
#endif
            Logger l = (Logger)log.Logger;

            l.RemoveAppender( appender );
            l.Repository.Configured = true; //critical line
        }
    }

    public interface IDynamicAppender : IDisposable
    {
        ILog Log { get; }
    }

    public class DynamicFileAppender : IDynamicAppender
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
#if NET45
            _log = LogManager.GetLogger( loggerName );
#else
            Repository.ILoggerRepository repo = LogManager.GetRepository( System.Reflection.Assembly.GetCallingAssembly() );
            _log = LogManager.GetLogger( repo.Name, loggerName );
#endif
        }

        public ILog Log { get { return _log; } }

        public void Dispose()
        {
            _appender.Close();
            FileAppenderFactory.RemoveAppender( _loggerName, _appender );
            _log = null;
        }
    }

    public class RollingFileAppenderHelper : IDynamicAppender
    {
        string _loggerName = string.Empty;
        IAppender _appender = null;
        ILog _log = null;

        public RollingFileAppenderHelper(string loggerName, string appenderName,
            string logfileName, string conversionPattern, string levelName = "ALL")
        {
            _loggerName = loggerName;
            _appender = FileAppenderFactory.CreateRollingFileAppender( appenderName, logfileName, conversionPattern );
            FileAppenderFactory.AddAppender( loggerName, levelName, _appender );
#if NET45
            _log = LogManager.GetLogger( loggerName );
#else
            Repository.ILoggerRepository repo = LogManager.GetRepository( System.Reflection.Assembly.GetCallingAssembly() );
            _log = LogManager.GetLogger( repo.Name, loggerName );
#endif
        }

        public ILog Log { get { return _log; } }

        public void Dispose()
        {
            _appender.Close();
            FileAppenderFactory.RemoveAppender( _loggerName, _appender );
            _log = null;
        }
    }
}