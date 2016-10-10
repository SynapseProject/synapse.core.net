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
        override public HandlerResult Execute(string parms, bool dryRun = false)
        {
            return new HandlerResult() { Status = StatusType.None };
        }

        protected string getMsg(StatusType status, bool dryRun)
        {
            return string.Format( "{0}-->dryRun:{1}", status, dryRun );
        }

        protected void WriteFile(string handler, string message)
        {
            string fn = $"{handler}_{Guid.NewGuid()}";
            System.IO.File.AppendAllText( fn, message ); ;
        }
    }

    public class UriHandler : HandlerRuntimeBase
    {
        override public HandlerResult Execute(string parms, bool dryRun = false)
        {
            string result = GetUri( parms ).Result;
            return new HandlerResult() { Status = StatusType.None };
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
        override public HandlerResult Execute(string parms, bool dryRun = false)
        {
            System.Threading.Thread.Sleep( 9000 );
            StatusType st = StatusType.Failed;
            bool cancel = OnProgress( "FooExecute", getMsg( StatusType.Initializing, dryRun ), StatusType.Initializing );
            if( !cancel )
            {
                OnProgress( "FooExecute", getMsg( StatusType.Running, dryRun ), StatusType.Running );
                if( !dryRun ) { OnProgress( "FooExecute", "...Progress...", StatusType.Running ); }
                OnProgress( "FooExecute", "Finished", st );
            }
            else
            {
                st = StatusType.Cancelled;
                OnProgress( "FooExecute", "Cancelled", st );
            }
            WriteFile( "FooHandler", $"parms:{parms}\r\nstatus:{st}" );
            return new HandlerResult() { Status = st };
        }
    }

    public class BarHandler : EmptyHandler
    {
        override public HandlerResult Execute(string parms, bool dryRun = false)
        {
            System.Threading.Thread.Sleep( 9000 );
            StatusType st = StatusType.Complete;
            bool cancel = OnProgress( "BarExecute", getMsg( StatusType.Initializing, dryRun ), StatusType.Initializing );
            if( !cancel )
            {
                OnProgress( "BarExecute", getMsg( StatusType.Running, dryRun ), StatusType.Running );
                if( !dryRun ) { OnProgress( "BarExecute", "...Progress...", StatusType.Running ); }
                OnProgress( "BarExecute", "Finished", st );
            }
            else
            {
                st = StatusType.Cancelled;
                OnProgress( "BarExecute", "Cancelled", st );
            }
            WriteFile( "BarHandler", $"parms:{parms}\r\nstatus:{st}" );
            return new HandlerResult() { Status = st };
        }
    }
}