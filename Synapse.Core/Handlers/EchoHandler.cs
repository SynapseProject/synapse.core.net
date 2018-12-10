using System;
using System.Text;

using Synapse.Core;
using Synapse.Core.Utilities;

public class EchoHandler : HandlerRuntimeBase
{
    string _config = null;

    public override object GetConfigInstance() { return null; }
    public override object GetParametersInstance() { return null; }

    public override IHandlerRuntime Initialize(string config)
    {
        _config = config;
        return base.Initialize( config );
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        EchoHandlerResult r = new EchoHandlerResult( startInfo, _config );
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            ExitData = r
        };

        OnProgress( "Execute", result.ExitData.ToString(), result.Status, startInfo.InstanceId, Int32.MaxValue );

        return result;
    }
}

public class EchoHandlerResult : HandlerStartInfo
{
    public EchoHandlerResult(HandlerStartInfo hsi, string config)
    {
        InstanceId = hsi.InstanceId;
        IsDryRun = hsi.IsDryRun;
        RequestUser = hsi.RequestUser;
        RequestNumber = hsi.RequestNumber;
        ParentExitData = hsi.ParentExitData;
        Parameters = hsi.Parameters;
        RunAs = hsi.RunAs;
        Config = config;
        CurrentPrincipal = $"{Environment.UserDomainName}\\{Environment.UserName}";
    }

    public string Config { get; internal set; }
    public string CurrentPrincipal { get; internal set; }

    public override string ToString()
    {
        StringBuilder s = new StringBuilder();

        s.AppendFormat( "InstanceId: {0}\r\n", InstanceId );
        s.AppendFormat( "IsDryRun: {0}\r\n", IsDryRun );
        s.AppendFormat( "RequestUser: {0}\r\n", RequestUser );
        s.AppendFormat( "RequestNumber: {0}\r\n", RequestNumber );
        s.AppendFormat( "ParentExitData: {0}\r\n", ParentExitData );
        s.AppendFormat( "RunAs: {0}\r\n", RunAs );
        s.AppendLine( $"CurrentPrincipal:{CurrentPrincipal}" );
        s.AppendLine( "Config:" );
        s.AppendLine( Config );
        s.AppendLine( "Parameters:" );
        s.AppendLine( Parameters );

        return s.ToString();
    }

    public string ToYaml()
    {
        return YamlHelpers.Serialize( this );
    }
}