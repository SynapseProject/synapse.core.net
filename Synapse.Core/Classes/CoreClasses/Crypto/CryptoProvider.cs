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

    //public class CryptoKeyInfo
    //{
    //    public string Uri { get; set; }
    //    public string ContainerName { get; set; }
    //    public CspProviderFlags CspFlags { get; set; } = CspProviderFlags.NoFlags;
    //}

    public class CryptoProvider
    {
        public void EnsureInitialized()
        {
            if( !HasProvider )
                Provider = new CryptoProviderInfo() { Type = CryptoProviderInfo.DefaultType };
            else if( !Provider.HasType )
                Provider.Type = CryptoProviderInfo.DefaultType;
        }

        public CryptoProviderInfo Provider { get; set; }
        [YamlIgnore]
        public bool HasProvider { get { return Provider != null; } }

        public ParameterInfo Parameters { get; set; }
        [YamlIgnore]
        public bool HasParameters { get { return Parameters != null; } }


        //public CryptoKeyInfo Key { get; set; }
        //[YamlIgnore]
        //public bool HasKey { get { return Key != null && !string.IsNullOrWhiteSpace( Key?.Uri ); } }

        public List<string> Elements { get; set; } = new List<string>();
        [YamlIgnore]
        public bool HasElements { get { return Elements != null && Elements?.Count > 0; } }

        public List<string> Errors { get; set; } = new List<string>();
        [YamlIgnore]
        public bool HasErrors { get { return Errors != null && Errors?.Count > 0; } }


        CryptoProvider _planCrypto = null;
        public void InheritSettingsIfRequired(CryptoProvider sourceProvider, CryptoInheritElementAction inheritElementAction = CryptoInheritElementAction.None)
        {
            _planCrypto = sourceProvider;
            sourceProvider?.EnsureInitialized();
            EnsureInitialized();

            if( !HasProvider )
                Provider = sourceProvider?.Provider?.Clone();
            if( !HasParameters )
                Parameters = sourceProvider?.Parameters?.Clone();


            //if( provider?.Key == null )
            //    return;

            //if( Key == null )
            //    Key = new CryptoKeyInfo();

            //if( string.IsNullOrWhiteSpace( Key.Uri ) || string.IsNullOrWhiteSpace( Key.ContainerName ) )
            //{
            //    Key.Uri = provider.Key.Uri;
            //    Key.ContainerName = provider.Key.ContainerName;
            //    Key.CspFlags = provider.Key.CspFlags;
            //}

            if( inheritElementAction != CryptoInheritElementAction.None )
            {
                if( inheritElementAction == CryptoInheritElementAction.Replace || Elements == null )
                    Elements = new List<string>();

                if( sourceProvider.Elements != null && sourceProvider.Elements.Count > 0 )
                    foreach( string el in sourceProvider.Elements )
                        Elements.Add( el );
            }
        }



        #region helper methods
        //[YamlIgnore]
        //public RSACryptoServiceProvider Rsa { get; private set; }

        //public void LoadRsaKeys()
        //{
        //    Rsa = CryptoHelpers.LoadRsaKeys( Key.ContainerName, Key.Uri, Key.CspFlags );
        //}

        public void CreateInstance()
        {
            EnsureInitialized();
            CryptoInstance = Provider.CreateRuntime( _planCrypto?.Provider.Type, _planCrypto, null );
        }

        [YamlIgnore]
        public ICryptoRuntime CryptoInstance { get; set; }

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
                if( !TryDecryptOrValue( s, out string decrypted ) )
                    response = Encrypt( s );
            }
            else
                response = Decrypt( s );

            return response;
        }

        internal string HandleCrypto(string s)
        {
            if( IsEncryptMode )
                return CryptoHelpers.Encrypt( CryptoInstance, s );
            else
                return CryptoHelpers.Decrypt( CryptoInstance, s );
        }


        public string Encrypt(string s)
        {
            return CryptoHelpers.Encrypt( CryptoInstance, s );
        }

        public string Decrypt(string s)
        {
            return CryptoHelpers.Decrypt( CryptoInstance, s );
        }

        public bool TryEncryptOrValue(string value, out string encryptedValue)
        {
            bool ok = false;
            encryptedValue = value;
            try
            {
                encryptedValue = CryptoHelpers.Encrypt( CryptoInstance, value );
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
                decryptedValue = CryptoHelpers.Decrypt( CryptoInstance, value );
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
                //Key = new CryptoKeyInfo()
                //{
                //    Uri = "Filepath to RSA key file; http support in future.",
                //    ContainerName = "RSA=supported container name",
                //    CspFlags = CspProviderFlags.NoFlags
                //}
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