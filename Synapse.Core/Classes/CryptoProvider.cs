using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Synapse.Core.Utilities;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public enum CryptoInheritElementAction
    {
        None,
        Merge,
        Replace
    }

    public class CryptoKeyInfo
    {
        public string Uri { get; set; }
        public string ContainerName { get; set; }
        public CspProviderFlags CspFlags { get; set; } = CspProviderFlags.NoFlags;
    }

    public class CryptoProvider
    {
        public CryptoKeyInfo Key { get; set; }

        public void InheritSettingsIfRequired(CryptoProvider provider, CryptoInheritElementAction inheritElementAction = CryptoInheritElementAction.None)
        {
            if( provider == null )
                return;

            if( Key == null )
                Key = new CryptoKeyInfo();

            if( string.IsNullOrWhiteSpace( Key.Uri ) || string.IsNullOrWhiteSpace( Key.ContainerName ) )
            {
                Key.Uri = provider.Key.Uri;
                Key.ContainerName = provider.Key.ContainerName;
                Key.CspFlags = provider.Key.CspFlags;
            }

            if( inheritElementAction != CryptoInheritElementAction.None )
            {
                if( inheritElementAction == CryptoInheritElementAction.Replace || Elements == null )
                    Elements = new List<string>();

                if( provider.Elements != null && provider.Elements.Count > 0 )
                    foreach( string el in provider.Elements )
                        Elements.Add( el );
            }
        }

        public List<string> Elements { get; set; } = new List<string>();

        public List<string> Errors { get; set; } = new List<string>();


        #region helper methods
        [YamlIgnore]
        public RSACryptoServiceProvider Rsa { get; private set; }

        public void LoadRsaKeys()
        {
            Rsa = CryptoHelpers.LoadRsaKeys( Key.ContainerName, Key.Uri, Key.CspFlags );
        }

        [YamlIgnore]
        internal bool IsEncryptMode { get; set; } = true;
        internal string SafeHandleCrypto(string s)
        {
            string response = s;

            if( IsEncryptMode )
            {
                //try to decrypt
                // - if success, then the value is already encrypted with these RSA keys, return orig string (s)
                // - if failure, needs to be encrypted
                string decrypted = null;
                if( !TryDecryptOrValue( s, out decrypted ) )
                    TryEncryptOrValue( s, out response );
            }
            else
                TryDecryptOrValue( s, out response );

            return response;
        }

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

        public bool TryEncryptOrValue(string value, out string encryptedValue)
        {
            bool ok = false;
            encryptedValue = value;
            try
            {
                encryptedValue = CryptoHelpers.Encrypt( Rsa, value );
                ok = true;
            }
            catch { }

            return ok;
        }

        public bool TryDecryptOrValue(string value, out string decryptedValue)
        {
            bool ok = false;
            decryptedValue = value;
            try
            {
                decryptedValue = CryptoHelpers.Decrypt( Rsa, value );
                ok = true;
            }
            catch { }

            return ok;
        }
        #endregion


        public static CryptoProvider CreateSample(bool addElements = true)
        {
            CryptoProvider cp = new CryptoProvider()
            {
                Key = new CryptoKeyInfo()
                {
                    Uri = "Filepath to RSA key file; http support in future.",
                    ContainerName = "RSA=supported container name",
                    CspFlags = CspProviderFlags.NoFlags
                }
            };
            cp.Errors = null;
            if( addElements )
            {
                cp.Elements.Add( "Element:IndexedElement[0]:Element" );
                cp.Elements.Add( "Element:IndexedElement[1]:Element" );
            }
            else
                cp.Elements = null;

            return cp;
        }
    }
}