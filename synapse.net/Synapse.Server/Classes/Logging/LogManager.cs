﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender.Dynamic;

namespace Synapse.Server
{
	public enum LogLevel
	{
		Debug,
		Info,
		Warn,
		Error,
		Fatal
	}

	public class LogManager : IDisposable
	{
		bool _disposed = false;

		public static readonly ILog Default = log4net.LogManager.GetLogger( "SynapseServer" );

		public LogManager()
		{
		}
		~LogManager()
		{
			Dispose();
		}

		public DynamicFileAppender GetDynamicFileAppender(string loggerName, string appenderName,
			string logfileName, string conversionPattern, string levelName)
		{
			return new DynamicFileAppender( loggerName, appenderName, logfileName, conversionPattern, levelName );
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

		public void Write(object message, ILog logger = null)
		{
			Write( LogLevel.Info, message );
		}

		public void Write(LogLevel level, object message, Exception ex = null, ILog logger = null)
		{
			if( logger == null )
			{
				logger = Default;
			}

			if( ex != null && (level == LogLevel.Debug || level == LogLevel.Info) )
			{
				level = LogLevel.Error;
			}

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
				log4net.LogManager.Shutdown();
			}
			_disposed = true;

			GC.SuppressFinalize( this );
		}
		#endregion
	}
}
