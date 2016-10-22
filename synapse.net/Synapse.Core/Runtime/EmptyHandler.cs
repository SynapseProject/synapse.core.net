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
        override public ExecuteResult Execute(string parms, HandlerStartInfo startInfo, bool dryRun = false)
        {
            return new ExecuteResult() { Status = StatusType.None };
        }

        protected string getMsg(StatusType status, bool dryRun)
        {
            return string.Format( "{0}-->dryRun:{1}", status, dryRun );
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
        override public ExecuteResult Execute(string parms, HandlerStartInfo startInfo, bool dryRun = false)
        {
            string result = GetUri( parms ).Result;
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
        override public ExecuteResult Execute(string parms, HandlerStartInfo startInfo, bool dryRun = false)
        {
            int seq = 1;
            StatusType st = StatusType.Failed;

            try
            {
                Dictionary<object, object> p = Utilities.MergeHelpers.DeserializeYaml( parms );
                st = (StatusType)Enum.Parse( typeof( StatusType ), p.Values.ElementAt( 0 ).ToString() );
            }
            catch { }

            System.Threading.Thread.Sleep( 500 );
            bool cancel = OnProgress( "FooExecute", getMsg( StatusType.Initializing, dryRun ), StatusType.Initializing, startInfo.InstanceId, seq++ );
            if( !cancel )
            {
                OnProgress( "FooExecute", getMsg( StatusType.Running, dryRun ), StatusType.Running, startInfo.InstanceId, seq++ );
                if( !dryRun ) { OnProgress( "FooExecute", "...Progress...", StatusType.Running, startInfo.InstanceId, seq++ ); }
                OnProgress( "FooExecute", "Finished", st, startInfo.InstanceId, seq++ );
            }
            else
            {
                st = StatusType.Cancelled;
                OnProgress( "FooExecute", "Cancelled", st, startInfo.InstanceId, seq++ );
            }
            WriteFile( "FooHandler", $"parms:{parms}\r\nstatus:{st}\r\n-->CurrentPrincipal:{System.Security.Principal.WindowsIdentity.GetCurrent().Name}" );
            return new ExecuteResult() { Status = st };
        }
    }

    public class BarHandler : EmptyHandler
    {
        override public ExecuteResult Execute(string parms, HandlerStartInfo startInfo, bool dryRun = false)
        {
            int seq = 1;
            System.Threading.Thread.Sleep( 500 );
            StatusType st = StatusType.Complete;
            bool cancel = OnProgress( "BarExecute", getMsg( StatusType.Initializing, dryRun ), StatusType.Initializing, startInfo.InstanceId, seq++ );
            if( !cancel )
            {
                OnProgress( "BarExecute", getMsg( StatusType.Running, dryRun ), StatusType.Running, startInfo.InstanceId, seq++ );
                if( !dryRun ) { OnProgress( "BarExecute", "...Progress...", StatusType.Running, startInfo.InstanceId, seq++ ); }
                OnProgress( "BarExecute", "Finished", st, startInfo.InstanceId, seq++ );
            }
            else
            {
                st = StatusType.Cancelled;
                OnProgress( "BarExecute", "Cancelled", st, startInfo.InstanceId, seq++ );
            }
            WriteFile( "BarHandler", $"parms:{parms}\r\nstatus:{st}\r\n-->CurrentPrincipal:{System.Security.Principal.WindowsIdentity.GetCurrent().Name}" );
            return new ExecuteResult() { Status = st };
        }
    }
}