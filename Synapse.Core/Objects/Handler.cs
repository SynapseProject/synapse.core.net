using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core
{
	public class HandlerInfo
	{
		public string Type { get; set; }
		public string ConfigKey { get; set; }
		public string ConfigValues { get; set; }
	}

	public class HandlerRuntime
	{
		abstract public bool Activate(string config);
		abstract public HandlerResult Execute();
	}

	public interface IHandlerConfig
	{
		string Key { get; set; }
	}

	public class HandlerResult
	{
		public static readonly HandlerResult Emtpy;
		public bool IsEmpty { get { return this == HandlerResult.Emtpy; } }
		public bool Success { get; set; }
		public int ExitCode { get; set; }
	}
}