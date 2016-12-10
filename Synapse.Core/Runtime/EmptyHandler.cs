using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        override public ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            return new ExecuteResult() { Status = StatusType.None, ExitData = "empty" };
        }

        protected string getMsg(StatusType status, HandlerStartInfo si)
        {
            return $"{status}-->dryRun:{si.IsDryRun}, ReqNum:{si.RequestNumber}";
        }

        protected void WriteFile(string handler, string message)
        {
            //string user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split( '\\' )[1];
            //string fn = $"{ActionName}_{user}_{handler}_{DateTime.Now.Ticks}_{Guid.NewGuid()}";
            //System.IO.File.AppendAllText( fn, message ); ;
        }
    }

    public class UriHandler : HandlerRuntimeBase
    {
        override public ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            string result = GetUri( startInfo.Parameters ).Result;
            return new ExecuteResult() { Status = StatusType.None };
        }

        async Task<string> GetUri(string uri)
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync( uri );
        }

        async Task<string> GetUri_x(string uri)
        {
            string result = null;

            using( HttpClient client = new HttpClient() )
            using( HttpResponseMessage response = await client.GetAsync( uri ) )
            using( HttpContent content = response.Content )
            {
                result = await content.ReadAsStringAsync();
            }

            return result;
        }
    }

    public class FooHandler : EmptyHandler
    {
        override public ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            int seq = 1;
            StatusType st = StatusType.Failed;

            try
            {
                Dictionary<object, object> p = Utilities.YamlHelpers.Deserialize( startInfo.Parameters );
                st = (StatusType)Enum.Parse( typeof( StatusType ), p.Values.ElementAt( 0 ).ToString() );
            }
            catch { }

            string x = startInfo.ParentExitData;

            System.Threading.Thread.Sleep( 500 );
            bool cancel = OnProgress( "FooExecute", getMsg( StatusType.Initializing, startInfo ), StatusType.Initializing, startInfo.InstanceId, seq++ );
            OnLogMessage( "FooExecute", $"   ----------   {startInfo.ParentExitData}   ---------- working ----------" );
            if( !cancel )
            {
                OnProgress( "FooExecute", getMsg( StatusType.Running, startInfo ), StatusType.Running, startInfo.InstanceId, seq++ );
                if( !startInfo.IsDryRun ) { OnProgress( "FooExecute", "...Progress...", StatusType.Running, startInfo.InstanceId, seq++ ); }
                //throw new Exception( "quitting" );
                OnProgress( "FooExecute", "Finished", st, startInfo.InstanceId, seq++ );
            }
            else
            {
                st = StatusType.Cancelled;
                OnProgress( "FooExecute", "Cancelled", st, startInfo.InstanceId, seq++ );
            }
            WriteFile( "FooHandler", $"parms:{startInfo.Parameters}\r\nstatus:{st}\r\n-->CurrentPrincipal:{System.Security.Principal.WindowsIdentity.GetCurrent().Name}" );
            return new ExecuteResult() { Status = st, ExitData = "foo" };
        }
    }

    public class BarHandler : EmptyHandler
    {
        override public ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            int seq = 1;

            string x = startInfo.ParentExitData;

            System.Threading.Thread.Sleep( 500 );
            StatusType st = StatusType.Complete;
            bool cancel = OnProgress( "BarExecute", getMsg( StatusType.Initializing, startInfo ), StatusType.Initializing, startInfo.InstanceId, seq++ );
            OnLogMessage( "BarExecute", $"   ----------   {startInfo.ParentExitData}   ---------- working ----------" );
            if( !cancel )
            {
                OnProgress( "BarExecute", getMsg( StatusType.Running, startInfo ), StatusType.Running, startInfo.InstanceId, seq++ );
                if( !startInfo.IsDryRun ) { OnProgress( "BarExecute", "...Progress...", StatusType.Running, startInfo.InstanceId, seq++ ); }
                OnProgress( "BarExecute", "Finished", st, startInfo.InstanceId, seq++ );
            }
            else
            {
                st = StatusType.Cancelled;
                OnProgress( "BarExecute", "Cancelled", st, startInfo.InstanceId, seq++ );
            }
            WriteFile( "BarHandler", $"parms:{startInfo.Parameters}\r\nstatus:{st}\r\n-->CurrentPrincipal:{System.Security.Principal.WindowsIdentity.GetCurrent().Name}" );
            return new ExecuteResult() { Status = st, ExitData = "bar" };
        }
    }
}