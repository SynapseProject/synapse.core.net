using System;

namespace Synapse.Core
{
	public enum StatusType
	{
		None = 0,
		New = 1,
		Initializing = 2,
		Running = 3,
		Complete = 4,
		CompletedWithErrors = 5,
		Waiting = 6,
		Failed = 7,
		Cancelling = 8,
		Cancelled = 9
	}
}