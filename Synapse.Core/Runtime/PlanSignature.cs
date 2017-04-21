using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public partial class Plan
    {
        public void Sign(string keyContainerName, string filePath, CspProviderFlags flags)
        {
            SHA1Managed hash = new SHA1Managed();
            RSACryptoServiceProvider rsa = CryptoHelpers.LoadRsaKeys( keyContainerName, filePath, flags );

            //initialize Signature to null so any existing value isn't computed into the hash.
            Signature = null;

            byte[] planBytes = Encoding.UTF8.GetBytes( ToYaml() );
            byte[] hashedData = hash.ComputeHash( planBytes );
            byte[] signature = rsa.SignHash( hashedData, CryptoConfig.MapNameToOID( "SHA1" ) );

            Signature = CryptoHelpers.Encode( signature );
        }

        public bool VerifySignature(string keyContainerName, string filePath, CspProviderFlags flags)
        {
            SHA1Managed hash = new SHA1Managed();
            RSACryptoServiceProvider rsa = CryptoHelpers.LoadRsaKeys( keyContainerName, filePath, flags );

            //the data was signed /without/ Signature having a value, so remove/cache the value
            string sig = Signature;
            Signature = null;

            byte[] signature = CryptoHelpers.DecodeToBytes( sig );
            byte[] planBytes = Encoding.UTF8.GetBytes( ToYaml() );
            bool ok = rsa.VerifyData( planBytes, CryptoConfig.MapNameToOID( "SHA1" ), signature );
            if( ok )
            {
                byte[] hashedData = hash.ComputeHash( planBytes );
                ok = rsa.VerifyHash( hashedData, CryptoConfig.MapNameToOID( "SHA1" ), signature );
            }

            //put the Signature back in place.
            Signature = sig;

            return ok;
        }

        public Plan EncryptElements()
        {
            return YamlHelpers.HandlePlanCrypto( this );
        }

        public Plan DecryptElements()
        {
            return YamlHelpers.HandlePlanCrypto( this, isEncryptMode: false );
        }
    }
}