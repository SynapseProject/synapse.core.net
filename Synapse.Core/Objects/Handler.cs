using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
	public class HandlerInfo
	{
		public string Type { get; set; }
		public Config Config { get; set; }
		[YamlIgnore]
		public bool HasConfig { get { return Config != null; } }
	}

	public interface IHandlerRuntime
	{
		IHandlerRuntime Initialize(string config);
		HandlerResult Execute(string parms); //maybe should be object

		event EventHandler<HandlerProgressCancelEventArgs> StepStarting;
		event EventHandler<HandlerProgressEventArgs> StepProgress;
		event EventHandler<HandlerProgressEventArgs> StepFinished;
	}

	public abstract class HandlerRuntimeBase : IHandlerRuntime
	{
		//public abstract string Parameters { get; set; }
		public abstract HandlerResult Execute(string parms);

		public virtual IHandlerRuntime Initialize(string config)
		{
			return this;
		}

		public event EventHandler<HandlerProgressCancelEventArgs> StepStarting;
		public event EventHandler<HandlerProgressEventArgs> StepProgress;
		public event EventHandler<HandlerProgressEventArgs> StepFinished;

		/// <summary>
		/// Notify of step start. If return value is True, then cancel operation.
		/// </summary>
		/// <param name="context">The method name or workflow activty.</param>
		/// <param name="message">Descriptive message.</param>
		/// <param name="status">Overall Package status indicator.</param>
		/// <param name="id">Message Id.</param>
		/// <param name="severity">Message/error severity.</param>
		/// <param name="ex">Current exception (optional).</param>
		/// <returns>HandlerProgressCancelEventArgs.Cancel value.</returns>
		protected virtual bool OnStepStarting(string context, string message, StatusType status = StatusType.Running, int id = 0, int severity = 0, bool cancel = false, Exception ex = null)
		{
			HandlerProgressCancelEventArgs e =
				new HandlerProgressCancelEventArgs( context, message, status, id, severity, cancel, ex );
			OnStepStarting( e );

			return e.Cancel;
		}

		/// <summary>
		/// Notify of step start. If e.Cancel is True, then cancel operation.
		/// </summary>
		protected virtual void OnStepStarting(HandlerProgressCancelEventArgs e)
		{
			if( StepStarting != null )
			{
				StepStarting( this, e );
			}
		}

		/// <summary>
		/// Notify of step progress.
		/// </summary>
		/// <param name="context">The method name or workflow activty.</param>
		/// <param name="message">Descriptive message.</param>
		/// <param name="status">Overall Package status indicator.</param>
		/// <param name="id">Message Id.</param>
		/// <param name="severity">Message/error severity.</param>
		/// <param name="ex">Current exception (optional).</param>
		protected virtual void OnStepProgress(string context, string message, StatusType status = StatusType.Running, int id = 0, int severity = 0, Exception ex = null)
		{
			if( StepProgress != null )
			{
				StepProgress( this, new HandlerProgressEventArgs( context, message, status, id, severity, ex ) );
			}
		}

		/// <summary>
		/// Notify of step completion.
		/// </summary>
		/// <param name="context">The method name or workflow activty.</param>
		/// <param name="message">Descriptive message.</param>
		/// <param name="status">Overall Package status indicator.</param>
		/// <param name="id">Message Id.</param>
		/// <param name="severity">Message/error severity.</param>
		/// <param name="ex">Current exception (optional).</param>
		protected virtual void OnStepFinished(string context, string message, StatusType status = StatusType.Running, int id = 0, int severity = 0, Exception ex = null)
		{
			if( StepFinished != null )
			{
				StepFinished( this, new HandlerProgressEventArgs( context, message, status, id, severity, ex ) );
			}
		}
	}

	public interface ICancelEventArgs
	{
		bool Cancel { get; set; }
	}
	public class HandlerProgressEventArgs : EventArgs
	{
		public HandlerProgressEventArgs(string context, string message,
			StatusType status = StatusType.Running, int id = 0, int severity = 0, Exception ex = null)
		{
			Context = context;
			Message = message;
			Status = status;
			Id = id;
			Severity = severity;
			Exception = ex;
		}

		public string Context { get; protected set; }
		public string Message { get; protected set; }
		public StatusType Status { get; protected set; }
		public int Id { get; protected set; }
		public int Severity { get; protected set; }
		public Exception Exception { get; protected set; }
		public bool HasException { get { return this.Exception != null; } }
	}
	public class HandlerProgressCancelEventArgs : HandlerProgressEventArgs, ICancelEventArgs
	{
		public HandlerProgressCancelEventArgs(string context, string message,
			StatusType status = StatusType.Running, int id = 0, int severity = 0,
			bool cancel = false, Exception ex = null)
			: base( context, message, status, id, severity, ex )
		{
			Cancel = cancel;
		}

		public bool Cancel { get; set; }
	}

	//public interface IHandlerConfig
	//{
	//	string Key { get; set; }
	//}

	public class HandlerResult
	{
		public HandlerResult()
		{
			Status = StatusType.None;
		}

		public static readonly HandlerResult Emtpy = new HandlerResult();
		public bool IsEmpty { get { return this == HandlerResult.Emtpy; } }

		public StatusType Status { get; set; }
		public object ExitData { get; set; }

		public override string ToString()
		{
			return Status.ToString();
		}
	}
}