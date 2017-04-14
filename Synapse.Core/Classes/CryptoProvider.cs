using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Synapse.Core.Utilities;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class CryptoProvider
    {
        public string PublicPrivateKeyFile { get; set; }
        public string PublicKeyFile { get; set; }
        public string KeyContainerName { get; set; }
        public CspProviderFlags CspFlags { get; set; } = CspProviderFlags.NoFlags;

        public List<string> Elements { get; set; }


        #region helper methods
        [YamlIgnore]
        public RSACryptoServiceProvider Rsa { get; private set; }

        public void LoadRsaKeys(bool forDecrypt = true)
        {
            _isDecrypt = forDecrypt;
            string keyFile = forDecrypt ? PublicKeyFile : PublicPrivateKeyFile;
            Rsa = CryptoHelpers.LoadRsaKeys( KeyContainerName, keyFile, CspFlags );
        }

        bool _isDecrypt = true;
        internal string HandleCrypto(string s)
        {
            if( _isDecrypt )
                return CryptoHelpers.Decrypt( Rsa, s );
            else
                return CryptoHelpers.Encrypt( Rsa, s );
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