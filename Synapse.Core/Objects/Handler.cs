using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core
{
	public class HandlerInfo
	{
		public string Type { get; set; }
		public Config Config { get; set; }
		public bool HasConfig { get { return Config != null; } }
	}

	public interface IHandlerRuntime
	{
		bool Initialize(string config);
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