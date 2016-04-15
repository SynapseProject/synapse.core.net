﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core.Runtime
{
	public class HandlerRuntimeFactory
	{
		public static IHandlerRuntime Create(HandlerInfo info)
		{
			IHandlerRuntime hr = new EmptyHandler();
			switch( info.Type.ToLower() )
			{
				case "foo":
				{
					hr = new FooHandler();
					break;
				}
				case "bar":
				{
					hr = new BarHandler();
					break;
				}
			}

			if( info.ConfigKey != null )
			{
				Dal.DataAccessLayer dal = new Dal.DataAccessLayer();
				string values = dal.GetHandlerConfig( info.ConfigKey );
				//merge values & info.ConfigValues
			}
			hr.Activate( info.ConfigValues );
			return hr;
		}
	}

	public class EmptyHandler : IHandlerRuntime
	{
		public bool Activate(string config)
		{
			return true;
		}

		public HandlerResult Execute(string parms)
		{
			return new HandlerResult() { Success = true, ExitCode = 0 };
		}
	}

	public class FooHandler : IHandlerRuntime
	{
		public bool Activate(string config)
		{
			return true;
		}

		public HandlerResult Execute(string parms)
		{
			return new HandlerResult() { Success = true, ExitCode = 1 };
		}
	}

	public class BarHandler : IHandlerRuntime
	{
		public bool Activate(string config)
		{
			return true;
		}

		public HandlerResult Execute(string parms)
		{
			return new HandlerResult() { Success = false, ExitCode = 2 };
		}
	}
}