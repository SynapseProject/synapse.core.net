using System;
using System.Collections.Generic;
using System.Text;

namespace Synapse.Common.WebApi
{
    public class Utilities
    {
        public static string UnwindException(Exception ex)
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine( ex.Message );
            if( ex.InnerException != null && ex.InnerException is AggregateException )
            {
                AggregateException ae = ex.InnerException as AggregateException;
                foreach( WebApiClientException wcx in ae.InnerExceptions )
                {
                    Stack<Exception> exceptions = new Stack<Exception>();
                    exceptions.Push( wcx );

                    while( exceptions.Count > 0 )
                    {
                        Exception e = exceptions.Pop();

                        if( e.InnerException != null )
                            exceptions.Push( e.InnerException );

                        msg.AppendLine( e.Message );

                        if( e is WebApiClientException && ((WebApiClientException)e).Details != null )
                            msg.AppendLine( ((WebApiClientException)e).Details.Message );
                    }
                }
            }

            return msg.ToString();
        }
    }
}