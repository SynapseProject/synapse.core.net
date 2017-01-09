using System;
using System.Net.Http.Formatting;
using System.Net.Mime;

namespace Synapse.Common.WebApi
{
    public class WebApiClientOptions
    {
        private string baseAddress = "http://localhost/";
        private uint timeout = 60000;
        private MediaTypeFormatter formatter = (MediaTypeFormatter) new JsonMediaTypeFormatter();
        private string contentType = "application/json";
        private IAuthentication authentication = (IAuthentication)new NoAuthentication();

        /// An object representing the desired type of authentication
        public IAuthentication Authentication
        {
            get
            {
                return this.authentication;
            }
            set
            {
                this.authentication = value;
            }
        }

        public string ContentType
        {
            get { return this.contentType; }
            set
            {
                if (value == "application/json" || value == "application/xml")
                {
                    this.contentType = value;
                }
            }
        }

        public MediaTypeFormatter Formatter
        {
            get { return this.formatter; }
        }

        public string BaseAddress
        {
            get { return this.baseAddress; }
            set { this.baseAddress = value.EndsWith("/") ? value : value + "/"; }
        }

        public uint Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        public WebApiClientOptions()
        {
        }

        public WebApiClientOptions(string baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress))
                throw new ArgumentNullException("baseAddress");

            this.BaseAddress = baseAddress;
        }
    }
}