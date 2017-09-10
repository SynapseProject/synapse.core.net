using System;
using System.Security;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    //todo: resolve config, use config to get decryption info, use u/p to auth to provider for impersonation thread
    public partial class SecurityContext : IInheritable
    {
        string _userName = null;
        bool _blockInheritance = false;
        bool _blockInheritanceIsSet = false;


        public SecurityContext()
        {
        }

        public string Domain { get; set; }
        /// <summary>
        /// The logon username.  If sepcified, BlockInheritance defaults to true.
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                if( !_blockInheritanceIsSet ) //don't userride a user-specified setting
                    _blockInheritance = !string.IsNullOrWhiteSpace( _userName );
            }
        }
        public string Password { get; set; }
        public string Provider { get; set; } //ad, aws, azure
        /// <summary>
        /// Indicates if UserName/Password have values set.
        /// </summary>
        [YamlIgnore]
        public bool IsValid { get { return !string.IsNullOrWhiteSpace( UserName ) && !string.IsNullOrWhiteSpace( Password ); } }
        /// <summary>
        /// Specifies if this SecurityContext block can be inherited by child SecurityContext blocks.
        /// </summary>
        public bool IsInheritable { get; set; } = true;
        /// <summary>
        /// Specifies if this SecurityContext instance will block inheritance from the parent SecurityContext instance.  Defaults to false unless UserName is specified.
        /// </summary>
        public bool BlockInheritance
        {
            get { return _blockInheritance; }
            set
            {
                _blockInheritance = value;
                _blockInheritanceIsSet = true;
            }
        }
        /// <summary>
        /// Indicates if the current settings are inherited from the parent SecurityContext block.
        /// </summary>
        public bool IsInherited { get; set; } = false;


        public ParameterInfo Config { get; set; }

        public CryptoProvider Crypto { get; set; }
        [YamlIgnore]
        public bool HasCrypto { get { return Crypto != null; } }


        public void InheritSettingsIfAllowed(SecurityContext sourceContext)
        {
            if( (sourceContext?.IsInheritable).Value && !this.BlockInheritance )
            {
                Domain = sourceContext.Domain;
                UserName = sourceContext.UserName;
                Password = sourceContext.Password;
                Provider = sourceContext.Provider;
                Config = sourceContext.Config?.Clone();
                Crypto = new CryptoProvider();
                Crypto.InheritSettingsIfRequired( sourceContext.Crypto, CryptoInheritElementAction.Replace );
                IsInheritable = true;
                IsInherited = true;
            }
            else
            {
                IsInherited = false;  //overwrite any user-declared setting
            }
        }


        public override string ToString()
        {
            return string.Format( $"[{Domain}]::[{UserName}]" );
        }

        public static SecurityContext CreateSample()
        {
            SecurityContext sc = new SecurityContext()
            {
                Domain = "AD Domain",
                UserName = "username",
                Password = "Use Crypto to Encrypt this value",
                Provider = "Reserved for future use: AD, AWS, Azure, etc.",

                Crypto = CryptoProvider.CreateSample()
            };

            return sc;
        }
    }
}