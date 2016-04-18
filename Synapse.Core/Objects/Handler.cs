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

	public interface IHandlerRuntime
	{
		bool Activate(string config);
		HandlerResult Execute(string parms); //maybe should be object
	}

	//public interface IHandlerConfig
	//{
	//	string Key { get; set; }
	//}

	public class HandlerResult
	{
		public HandlerResult()
		{
			Status = HandlerStatus.None;
		}

		public static readonly HandlerResult Emtpy = new HandlerResult();
		public bool IsEmpty { get { return this == HandlerResult.Emtpy; } }
		public static readonly int DefaultExitCode = -1;

		public HandlerStatus Status { get; set; }
		public object ExitData { get; set; }

		public override string ToString()
		{
			return Status.ToString();
		}
	}
}