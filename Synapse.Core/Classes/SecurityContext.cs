using System;
using System.Security;


namespace Synapse.Core
{
    //todo: resolve config, use config to get decryption info, use u/p to auth to provider for impersonation thread
    public partial class SecurityContext
    {
        public SecurityContext()
        {
        }

        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Provider { get; set; } //ad, aws, azure
        public ParameterInfo Config { get; set; }

        public override string ToString()
        {
            return string.Format( "{0}-->{1}", UserName, Password );
        }
    }
}