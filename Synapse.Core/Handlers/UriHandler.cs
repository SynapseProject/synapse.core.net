using System;
using System.Net.Http;
using System.Threading.Tasks;

using Synapse.Core;

namespace Synapse.Handlers
{
    public class UriHandler : HandlerRuntimeBase
    {
        override public ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            ExecuteResult result = new ExecuteResult() { Status = StatusType.Complete };
            string msg = string.Empty;
            Exception exception = null;

            UriHandlerParameters parms = DeserializeOrNew<UriHandlerParameters>( startInfo.Parameters );

            try
            {
                result.ExitData = GetUri( parms.Uri ).Result;
                msg = $"Successfully executed HttpClient.Get( {parms.Uri} ).";
            }
            catch( Exception ex )
            {
                result.Status = StatusType.Failed;
                msg = result.ExitData = ex.Message;
                exception = ex;
            }

            OnProgress( "Execute", msg, result.Status, startInfo.InstanceId, Int32.MaxValue, false, exception );

            return result;
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

    public class UriHandlerParameters
    {
        public UriHandlerParameters()
        {
        }

        public string Uri { get; set; }
    }
}