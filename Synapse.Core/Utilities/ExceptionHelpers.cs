using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;

namespace Synapse.Core.Utilities
{
    public class ExceptionHelpers
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
                    foreach( Exception wcx in ae.InnerExceptions )
                    {
                        Stack<Exception> exceptions = new Stack<Exception>();
                        exceptions.Push( wcx );

                        while( exceptions.Count > 0 )
                        {
                            Exception e = exceptions.Pop();

                            if( e.InnerException != null )
                                exceptions.Push( e.InnerException );

                            msg.Append( $"{e.Message}{lineEnd}" );
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

                        if( e.InnerException != null )
                            exceptions.Push( e.InnerException );

                        msg.Append( $"{e.Message}{lineEnd}" );
                    }
                }
            }

            return asSingleLine ? msg.ToString().TrimEnd( '|' ) : msg.ToString();
        }
    }




    public static class ExceptionExtensions
    {
        public static Exception ToFriendlyYamlException(this Exception ex, string message = null, string yaml = null)
        {
            if( ex is YamlException yx )
            {
                string err =  null;
                if( !string.IsNullOrWhiteSpace( yaml ) )
                    err = $"\r\nText of range is: {yaml.Substring( yx.Start.Index, yx.End.Index - yx.Start.Index )}";

                string msg = $"Error encountered with YAML between line {yx.Start.Line}/ch {yx.Start.Column} and line {yx.End.Line}/ch {yx.End.Column}, and between document indexes {yx.Start.Index} and {yx.End.Index}.{err}\r\n{message}";

                return new Exception( message: msg, innerException: yx );
            }
            else
                return ex;
        }
    }
}