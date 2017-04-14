using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Synapse.Core.Utilities
{
    public class CryptoHelpers
    {
        #region base64
        public static string Encode(string value)
        {
            byte[] valueBytes = ASCIIEncoding.ASCII.GetBytes( value );
            return Convert.ToBase64String( valueBytes );
        }

        public static string Encode(byte[] valueBytes)
        {
            return Convert.ToBase64String( valueBytes );
        }

        public static string Decode(string value)
        {
            byte[] valueBytes = Convert.FromBase64String( value );
            return ASCIIEncoding.ASCII.GetString( valueBytes );
        }

        public static byte[] DecodeToBytes(string value)
        {
            return Convert.FromBase64String( value );
        }

        public static bool TryDecode(string encodedValue, out string decodedValue)
        {
            try
            {
                byte[] valueBytes = Convert.FromBase64String( encodedValue );
                decodedValue = ASCIIEncoding.ASCII.GetString( valueBytes );
                return true;
            }
            catch
            {
                decodedValue = null;
                return false;
            }
        }
        #endregion


        #region rsa
        public static void GenerateRsaKeys(string keyContainerName, string pubPrivFilePath, string pubOnlyFilePath)
        {
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = keyContainerName;
            GenerateRsaKeys( cspParams, pubPrivFilePath, pubOnlyFilePath );
        }
        public static void GenerateRsaKeys(CspParameters cspParams, string pubPrivFilePath, string pubOnlyFilePath)
        {
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );

            if( !string.IsNullOrEmpty( pubPrivFilePath ) )
            {
                using( StreamWriter sw = new StreamWriter( pubPrivFilePath ) )
                {
                    sw.Write( rsaKey.ToXmlString( true ) );
                }
            }

            if( !string.IsNullOrEmpty( pubOnlyFilePath ) )
            {
                using( StreamWriter sw = new StreamWriter( pubOnlyFilePath ) )
                {
                    sw.Write( rsaKey.ToXmlString( false ) );
                }
            }
        }

        public static RSACryptoServiceProvider LoadRsaKeys(string keyContainerName, string filePath)
        {
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = keyContainerName;
            return LoadRsaKeys( cspParams, filePath );
        }
        public static RSACryptoServiceProvider LoadRsaKeys(string keyContainerName, string filePath, CspProviderFlags flags)
        {
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = keyContainerName;
            cspParams.Flags = flags;
            return LoadRsaKeys( cspParams, filePath );
        }
        public static RSACryptoServiceProvider LoadRsaKeys(CspParameters cspParams, string filePath)
        {
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );
            if( System.IO.File.Exists( filePath ) )
            {
                using( StreamReader sr = new StreamReader( filePath ) )
                {
                    rsaKey.FromXmlString( sr.ReadToEnd() );
                }
                return rsaKey;
            }
            else
            {
                return null;
            }
        }

        public static string Encrypt(RSACryptoServiceProvider rsa, string value)
        {
            byte[] valueBytes = ASCIIEncoding.ASCII.GetBytes( value );
            byte[] encrypted = rsa.Encrypt( valueBytes, false );
            return Convert.ToBase64String( encrypted );
        }

        public static string Decrypt(RSACryptoServiceProvider rsa, string value)
        {
            byte[] valueBytes = Convert.FromBase64String( value );
            byte[] decrypted = rsa.Decrypt( valueBytes, false );
            return ASCIIEncoding.ASCII.GetString( decrypted );
        }
        #endregion
    }
}