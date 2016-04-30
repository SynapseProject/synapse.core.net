using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Synapse.Core.Runtime
{
	//public class HandlerRuntimeFactory
	//{
	//	public static IHandlerRuntime Create(HandlerInfo info)
	//	{
	//		IHandlerRuntime hr = new EmptyHandler();

	//		string[] typeInfo = info.Type.Split( ':' );
	//		AssemblyName an = new AssemblyName( typeInfo[0] );
	//		Assembly hrAsm = Assembly.Load( an );
	//		Type handlerRuntime = hrAsm.GetType( typeInfo[1], true );
	//		hr = Activator.CreateInstance( handlerRuntime ) as IHandlerRuntime;

	//		string config = info.HasConfig ? info.Config.ResolvedValuesSerialized : null;
	//		hr.Initialize( config );

	//		return hr;
	//	}
	//}

	public class EmptyHandler : HandlerRuntimeBase
	{
		override public HandlerResult Execute(string parms)
		{
			return new HandlerResult() { Status = StatusType.None };
		}
	}

	public class FooHandler : HandlerRuntimeBase
	{
		override public HandlerResult Execute(string parms)
		{
			bool cancel = OnStepStarting( "FooExecute", "Starting" );
			if( !cancel )
			{
				OnStepProgress( "FooExecute", "Progress" );
				OnStepFinished( "FooExecute", "Finished" );
			}
			else
			{
				OnStepFinished( "FooExecute", "Cancelled" );
			}
			return new HandlerResult() { Status = StatusType.Failed };
		}
	}

	public class BarHandler : HandlerRuntimeBase
	{
		override public HandlerResult Execute(string parms)
		{
			bool cancel = OnStepStarting( "BarExecute", "Starting" );
			if( !cancel )
			{
				OnStepProgress( "BarExecute", "Progress" );
				OnStepFinished( "BarExecute", "Finished" );
			}
			else
			{
				OnStepFinished( "BarExecute", "Cancelled" );
			}
			return new HandlerResult() { Status = StatusType.Complete };
		}
	}
}