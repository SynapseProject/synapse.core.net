using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Synapse.Core.Utilities;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class CryptoProvider
    {
        public string KeyUri { get; set; }
        public string KeyContainerName { get; set; }
        public CspProviderFlags CspFlags { get; set; } = CspProviderFlags.NoFlags;

        public List<string> Elements { get; set; }


        #region helper methods
        [YamlIgnore]
        public RSACryptoServiceProvider Rsa { get; private set; }

        public void LoadRsaKeys()
        {
            Rsa = CryptoHelpers.LoadRsaKeys( KeyContainerName, KeyUri, CspFlags );
        }

        [YamlIgnore]
        internal bool IsEncryptMode { get; set; } = true;
        internal string HandleCrypto(string s)
        {
            if( IsEncryptMode )
                return CryptoHelpers.Encrypt( Rsa, s );
            else
                return CryptoHelpers.Decrypt( Rsa, s );
        }

        public string Encrypt(string s)
        {
            return CryptoHelpers.Encrypt( Rsa, s );
        }

        public string Decrypt(string s)
        {
            return CryptoHelpers.Decrypt( Rsa, s );
        }
        #endregion
    }
}