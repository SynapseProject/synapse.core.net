using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core.Runtime
{
	public 	class HandlerRuntimeFactory
	{
		public static HandlerRuntime Create(HandlerInfo info)
		{
			HandlerRuntime hr = new HandlerRuntime();
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
}