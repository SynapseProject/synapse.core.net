﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Server
{
	public enum ServiceStatus
	{
		Starting,
		Running,
		Pausing,
		Paused,
		Resuming,
		Stopping,
		Stopped,
		Disabled,
		Recovering,
		DrainStopping
	};
}