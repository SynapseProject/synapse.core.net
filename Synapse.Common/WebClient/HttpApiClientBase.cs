using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;


namespace Synapse.Common.WebApi
{
    public abstract partial class HttpApiClientBase
    {
        protected HttpClient Client;

        public HttpRequestHeaders Headers { get; private set; }

        public WebApiClientOptions Options { get; private set; }

        public HttpClientHandler Handler { get; private set; }

        public HttpApiClientBase(string baseUrl, string messageFormatType = "application/json")
        {
            WebApiClientOptions options = new WebApiClientOptions()
            {
                BaseAddress = baseUrl,
                ContentType = messageFormatType,
            };

            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                UseProxy = false,
                PreAuthenticate = true
            };

            this.Options = options;
            this.Handler = handler;
            this.Client = new HttpClient( (HttpMessageHandler)handler );
            this.Client.BaseAddress = new Uri( this.Options.BaseAddress );
            this.Headers = this.Client.DefaultRequestHeaders;
            this.Headers.Accept.Clear();
            this.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( this.Options.ContentType ) );
        }

        #region Generic
        /// Gets the Authorization header
        private void Authenticate()
        {
            this.Client.DefaultRequestHeaders.Authorization = this.Options.Authentication.Authenticate();
        }

        private WebApiClientException GetException(HttpResponseMessage response)
        {
            WebApiClientExceptionDetails details = new WebApiClientExceptionDetails();
            string result = response.Content.ReadAsStringAsync().Result;
            try
            {
                Dictionary<string, IList<string>> dictionary = (Dictionary<string, IList<string>>)null;
                JObject jobject = JObject.Parse( result );
                if( (IDictionary<string, JToken>)jobject["ModelState"] != null )
                    dictionary = jobject["ModelState"].ToObject<Dictionary<string, IList<string>>>();
                string str1 = (string)jobject["Message"];
                string str2 = (string)jobject["ExceptionMessage"];
                details.Message = str2 ?? str1;
                details.ExceptionType = (string)jobject["ExceptionType"];
                details.StackTrace = (string)jobject["StackTrace"];
                details.ModelState = dictionary;
            }
            catch
            {
                if( !string.IsNullOrEmpty( result ) )
                    details.Message = result;
            }
            details.Reason = response.ReasonPhrase;
            return new WebApiClientException( response.StatusCode, details );
        }

        protected virtual async Task DeleteAsync(string requestUri)
        {
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                HttpResponseMessage response = await this.Client.DeleteAsync( requestUri ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
        }

        protected virtual async Task GetAsync(string requestUri)
        {
            object obj;
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                HttpResponseMessage response = await this.Client.GetAsync( requestUri ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
                obj = await response.Content.ReadAsAsync<object>().ConfigureAwait( false );
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
        }

        protected virtual async Task<T> GetAsync<T>(string requestUri)
        {
            T obj;
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                HttpResponseMessage response = await this.Client.GetAsync( requestUri ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
                obj = await response.Content.ReadAsAsync<T>().ConfigureAwait( false );
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
            return obj;
        }

        protected virtual async Task PostAsyncVoid<T>(T obj, string requestUri)
        {
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                ObjectContent<T> content = new ObjectContent<T>( obj, this.Options.Formatter );
                await content.LoadIntoBufferAsync().ConfigureAwait( false );
                HttpResponseMessage response =
                    await this.Client.PostAsync( requestUri, (HttpContent)content ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
                try
                {
                    await response.Content.ReadAsAsync<T>().ConfigureAwait( false );
                }
                catch
                {
                    throw;
                }
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
        }

        protected virtual async Task<T> PostAsync<T>(T obj, string requestUri)
        {
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                ObjectContent<T> content = new ObjectContent<T>( obj, this.Options.Formatter );
                await content.LoadIntoBufferAsync().ConfigureAwait( false );
                HttpResponseMessage response =
                    await this.Client.PostAsync( requestUri, (HttpContent)content ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
                try
                {
                    return await response.Content.ReadAsAsync<T>().ConfigureAwait( false );
                }
                catch
                {
                    return default( T );
                }
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
        }

        protected virtual async Task<T1> PostAsync<T0, T1>(T0 obj, string requestUri)
        {
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                ObjectContent<T0> content = new ObjectContent<T0>( obj, this.Options.Formatter );
                await content.LoadIntoBufferAsync().ConfigureAwait( false );
                HttpResponseMessage response =
                    await this.Client.PostAsync( requestUri, (HttpContent)content ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
                try
                {
                    return await response.Content.ReadAsAsync<T1>().ConfigureAwait( false );
                }
                catch
                {
                    return default( T1 );
                }
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
        }

        protected virtual async Task<T> PutAsync<T>(T obj, string requestUri)
        {
            try
            {
                this.Authenticate(); // Placeholder in case we need to specify authentication header
                ObjectContent<T> content = new ObjectContent<T>( obj, this.Options.Formatter );
                await content.LoadIntoBufferAsync().ConfigureAwait( false );
                HttpResponseMessage response = await this.Client.PutAsync( requestUri, (HttpContent)content ).ConfigureAwait( false );
                if( !response.IsSuccessStatusCode )
                    throw this.GetException( response );
                try
                {
                    return await response.Content.ReadAsAsync<T>().ConfigureAwait( false );
                }
                catch
                {
                    return default( T );
                }
            }
            catch( WebApiClientException ex )
            {
                throw ex;
            }
            catch( Exception ex )
            {
                throw new WebApiClientException( HttpStatusCode.InternalServerError, ex );
            }
        }

        #endregion
    }
}