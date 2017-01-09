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
    public class NoAuthentication : IAuthentication
    {
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
            return (AuthenticationHeaderValue)null;
        }
    }
}
