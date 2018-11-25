﻿using System;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class SecurityContext : ICloneable<SecurityContext>, IInheritable, ICrypto
    {
        bool _blockInheritance = false;
        bool _blockInheritanceIsSet = false;

        public string Type { get; set; }
        [YamlIgnore]
        public bool HasType { get { return !string.IsNullOrWhiteSpace( Type ); } }

        public ParameterInfo Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return Config != null; } }

        public ParameterInfo Parameters { get; set; }
        [YamlIgnore]
        public bool HasParameters { get { return Parameters != null; } }

        [YamlIgnore]
        public bool IsDeclared { get { return HasType || HasConfig || HasParameters; } }

        #region IInheritable
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

        public void InheritSettingsIfAllowed(SecurityContext sourceContext)
        {
            if( sourceContext != null && sourceContext.IsInheritable && !this.BlockInheritance )
            {
                Config = sourceContext.Config?.Clone();
                Parameters = sourceContext.Parameters?.Clone();
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
        #endregion


        #region Clone, Sample
        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public SecurityContext Clone(bool shallow = true)
        {
            SecurityContext sc = (SecurityContext)MemberwiseClone();
            if( HasConfig )
                sc.Config = Config.Clone( shallow );
            if( HasParameters )
                sc.Parameters = Parameters.Clone( shallow );
            return sc;
        }

        public override string ToString()
        {
            return Type;
        }


        public static SecurityContext CreateSample()
        {
            SecurityContext handler = new SecurityContext()
            {
                Type = "Synapse.Handlers.CommandLine:CommandHandler",
                Config = ParameterInfo.CreateSample()
            };

            return handler;
        }
        #endregion


        public CryptoProvider Crypto { get; set; }
        [YamlIgnore]
        public bool HasCrypto { get { return Crypto != null; } }


        public SecurityContext GetCryptoValues(CryptoProvider planCrypto = null, bool isEncryptMode = true)
        {
            if( HasCrypto )
                Crypto.InheritSettingsIfRequired( planCrypto );
            return Utilities.YamlHelpers.GetCryptoValues( this, isEncryptMode );
        }
    }
}