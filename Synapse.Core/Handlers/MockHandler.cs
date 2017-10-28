using System;
using System.Collections.Generic;
using Synapse.Core;

public class MockAdHandler : HandlerRuntimeBase
{
    public override object GetConfigInstance() { return null; }
    public override object GetParametersInstance()
    {
        return new MockAdHandlerParameters()
        {
            SearchRequests = new List<SearchRequest>()
            {
                {
                    new SearchRequest()
                    {
                        Filter = "sample filter",
                        ReturnAttributes = new List<string>(){ "steve", "guy", "tom petty" }
                    }
                }
            }
        };
    }

    override public ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        MockAdHandlerParameters parms = DeserializeOrNew<MockAdHandlerParameters>( startInfo.Parameters );


        OnProgress( "Execute", StatusType.Success.ToString(), StatusType.Success, startInfo.InstanceId, Int32.MaxValue );

        return new ExecuteResult() { Status = StatusType.Success, ExitData = parms };
    }
}

public class MockAdHandlerParameters
{
    public MockAdHandlerParameters()
    {
    }

    public List<SearchRequest> SearchRequests { get; set; } = new List<SearchRequest>();
}

public class SearchRequest
{
    public string Filter { get; set; }
    public List<string> ReturnAttributes { get; set; } = new List<string>();
}