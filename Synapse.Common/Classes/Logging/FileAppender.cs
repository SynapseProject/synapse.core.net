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

        public static IAppender CreateRollingFileAppender(string appenderName, string logfileName, string conversionPattern)
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.Name = appenderName;
            appender.File = logfileName;
            appender.StaticLogFileName = true;
            appender.AppendToFile = false;
            appender.RollingStyle = RollingFileAppender.RollingMode.Size;
            appender.MaxSizeRollBackups = 10;
            appender.MaximumFileSize = "10MB";
            appender.PreserveLogFileNameExtension = true;

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = conversionPattern;
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            Config.BasicConfigurator.Configure( appender );

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
            _log = LogManager.GetLogger( loggerName );
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
            _log = LogManager.GetLogger( loggerName );
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