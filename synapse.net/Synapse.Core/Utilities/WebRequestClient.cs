using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Synapse.Core.Utilities
{
    public enum WebMessageFormatType
    {
        Json,
        Xml,
        Yaml
    }

    public struct HttpMethod
    {
        public const string Create = "POST";
        public const string Update = "PUT";
        public const string Delete = "DELETE";
        public const string Select = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Get = "GET";
        public const string Head = "HEAD";
        public const string Trace = "TRACE";
        public const string Options = "OPTIONS";
    }

    public class WebRequestClient
    {
        public string BaseUrl { get; set; }
        public WebMessageFormatType MessageFormat { get; set; }
        public int RequestTimeout { get; set; }
        public bool IsJson { get { return MessageFormat == WebMessageFormatType.Json; } }
        public string ContentType { get { return IsJson ? "application/json" : "application/xml"; } }

        public WebRequestClient()
        {
            this.MessageFormat = WebMessageFormatType.Json;
        }

        public WebRequestClient(string baseUrl, WebMessageFormatType messageFormat = WebMessageFormatType.Json)
        {
            this.BaseUrl = baseUrl;
            this.MessageFormat = messageFormat;
        }

        public T WebRequestSync<T>(Uri url, string method = HttpMethod.Get, byte[] data = null)
        {
            T result = default( T );

            WebRequest request = WebRequest.Create( url );
            request.Timeout = this.RequestTimeout == 0 ? (1000 * 60 * 5) : RequestTimeout;
            request.ContentType = this.ContentType;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = method;

            if( data != null && data.Length > 0 )
            {
                using( Stream requestStream = request.GetRequestStream() )
                {
                    requestStream.Write( data, 0, data.Length );
                }
            }

            WebResponse response = null;
            try
            {
                response = request.GetResponse();
            }
            catch( WebException wex )
            {
                throw wex.ToException();
            }
            catch { throw; }

            if( url.IsFile )
            {
                StreamReader responseStream = new StreamReader( response.GetResponseStream() );
                object foo = responseStream.ReadToEnd();  //boxing is stinky
                result = (T)foo;
            }
            else if( typeof( T ) != typeof( VoidObject ) )
            {
                XmlObjectSerializer dcs = new DataContractJsonSerializer( typeof( T ) );
                if( response.ContentType.ToLower().Contains( "xml" ) )
                {
                    dcs = new DataContractSerializer( typeof( T ) );
                }
                using( Stream rs = response.GetResponseStream() )
                {
                    try
                    {
                        result = (T)dcs.ReadObject( rs );
                    }
                    catch( SerializationException serex )
                    {
                        EvalSerializationException( ref serex, url, method );
                        throw;
                    }
                    catch { throw; }
                }
            }
            response.Close();

            return result;
        }

        void EvalSerializationException(ref SerializationException serex, Uri url, string method)
        {
            int hresultNullValueCode = -2146233076;

            serex.Data.Add( "SerializationException",
                string.Format( "Could not deserialize result from [{1}]: {0}.", url, method ) );
            serex.Data.Add( "HResult", serex.HResult );

            if( serex.HResult == hresultNullValueCode ||
                serex.Message.StartsWith( "Expecting element 'root' from namespace ''.. Encountered 'None'  with name '', namespace ''." ) )
            {
                serex.Data.Add( "Resolution", "Check unexpected void/null return type/value from method." );
            }
            else
            {
                serex.Data.Add( "Resolution", string.Format( "Unknown error.", url, method ) );
            }
        }

        public static string GetString(string uri, string method = HttpMethod.Get, WebMessageFormatType format = WebMessageFormatType.Json)
        {
            WebRequest request = WebRequest.Create( new Uri( uri ) );
            request.Timeout = (1000 * 60 * 5);
            request.ContentType = format == WebMessageFormatType.Json ? "application/json" : "application/xml";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = method;

            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                StreamReader responseStream = new StreamReader( response.GetResponseStream() );
                return responseStream.ReadToEnd();
            }
            catch( WebException wex )
            {
                throw wex.ToException();
            }
            catch { throw; }
        }
    }

    //surely there's a better way to handle this
    public class VoidObject { }
}