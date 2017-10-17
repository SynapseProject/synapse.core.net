using System;

using Synapse.Core;

public class EmptyHandler : HandlerRuntimeBase
{
    public override object GetConfigInstance() { return null; }
    public override object GetParametersInstance() { return new EmptyHandlerParameters() { ReturnStatus = StatusType.Success, SleepMilliseconds = 1000, ExitData = "Sample Value" }; }
    override public ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        EmptyHandlerParameters parms = DeserializeOrNew<EmptyHandlerParameters>( startInfo.Parameters );

        if( parms.SleepMilliseconds > 0 )
        {
            OnProgress( "Execute", $"Sleeping for {parms.SleepMilliseconds}", StatusType.Waiting, startInfo.InstanceId, 1 );
            System.Threading.Thread.Sleep( parms.SleepMilliseconds );
        }

        OnProgress( "Execute", parms.ReturnStatus.ToString(), parms.ReturnStatus, startInfo.InstanceId, Int32.MaxValue );

        return new ExecuteResult() { Status = parms.ReturnStatus, ExitData = parms.ExitData };
    }
}

public class EmptyHandlerParameters
{
    public EmptyHandlerParameters()
    {
        ReturnStatus = StatusType.None;
        ExitData = "EmptyHandler ExitData default value.";
    }

    public int SleepMilliseconds { get; set; }
    public StatusType ReturnStatus { get; set; }
    public object ExitData { get; set; }
}