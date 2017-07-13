using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Common.WebApi
{
    /// <summary>Defines no authentication</summary>
    public class BasicAuthentication : IAuthentication
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public BasicAuthentication() { }

        public BasicAuthentication(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        public BasicAuthentication(AuthenticationHeaderValue authHeader)
        {
            if ( authHeader != null )
            {
                if ( authHeader.Scheme?.ToLower() == "basic" )
                {
                    String userpass = authHeader.Parameter;
                    byte[] bytes = Convert.FromBase64String( userpass );
                    String decodedStr = Encoding.UTF8.GetString( bytes );
                    String[] parts = decodedStr.Split( ':' );
                    UserName = parts[0];
                    Password = parts[1];
                }
                else
                    throw new Exception( $"Invalid AuthenticationHeader Type [{authHeader.Scheme}] Provided." );
            }
        }

        /// <summary>Not used</summary>
        public ICredentials Credentials
        {
            get
            {
                return (ICredentials)null;
            }
        }

        /// <summary>Not necessary for this kind of authentication</summary>
        public AuthenticationHeaderValue Authenticate()
        {
            var byteArray = Encoding.ASCII.GetBytes( $"{UserName}:{Password}" );
            return new System.Net.Http.Headers.AuthenticationHeaderValue( "Basic", Convert.ToBase64String( byteArray ) );
        }
    }
}
