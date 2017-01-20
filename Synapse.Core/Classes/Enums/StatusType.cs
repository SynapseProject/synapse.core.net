using System;

namespace Synapse.Core
{
	public enum StatusType
	{
		None = 0,

        //'Active' states:
		New = 1,
		Initializing = 2,
		Running = 4,
        Waiting = 8,
        Cancelling = 16,

        //Terminal states:
        //Complete/Success share the same value intentionally,
        //  just to support friendly presentation.
        Complete = 128,
        Success = 128,
        CompletedWithErrors = 256,
        SuccessWithErrors = 256,
		Failed = 512,
		Cancelled = 1024,
        Tombstoned = 2048,

        Any = 16383
    }


    //public enum StatusType
    //{
    //	None = 0,
    //	New = 1,
    //	Initializing = 2,
    //	Running = 3,
    //	Complete = 4,
    //	CompletedWithErrors = 5,
    //	Waiting = 6,
    //	Failed = 7,
    //	Cancelling = 8,
    //	Cancelled = 9,
    //  Any = 10
    //}
}