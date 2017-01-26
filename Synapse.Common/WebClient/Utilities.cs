using System;
using System.Collections.Generic;
using System.Text;

namespace Synapse.Common.WebApi
{
    public class Utilities
    {
        public static string UnwindException(Exception ex)
        {
            return UnwindException( null, ex );
        }
        public static string UnwindException(string context, Exception ex, bool asSingleLine = false)
        {
            string lineEnd = asSingleLine ? "|" : @"\r\n";

            StringBuilder msg = new StringBuilder();
            if( !string.IsNullOrWhiteSpace( context ) )
                msg.Append( $"An error occurred in: {context}{lineEnd}" );

            msg.Append( $"{ex.Message}{lineEnd}" );

            if( ex.InnerException != null )
            {
                if( ex.InnerException is AggregateException )
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

                            msg.Append( $"{e.Message}{lineEnd}" );

                            if( e is WebApiClientException && ((WebApiClientException)e).Details != null )
                                msg.Append( $"{((WebApiClientException)e).Details.Message}{lineEnd}" );
                        }
                    }
                }
                else
                {
                    Stack<Exception> exceptions = new Stack<Exception>();
                    exceptions.Push( ex.InnerException );

                    while( exceptions.Count > 0 )
                    {
                        Exception e = exceptions.Pop();
                        msg.Append( $"{e.Message}{lineEnd}" );

                        if( e.InnerException != null )
                            exceptions.Push( e.InnerException );
                    }
                }
            }

            return asSingleLine ? msg.ToString().TrimEnd( '|' ) : msg.ToString();
        }
    }
}