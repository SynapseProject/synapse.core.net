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
        #region RsaKeys
        public static void GenerateRsaKeys(string keyContainerName = null, string pubPrivFilePath = null, string pubOnlyFilePath = null, int keySize = 0)
        {
            if( string.IsNullOrWhiteSpace( keyContainerName ) && string.IsNullOrWhiteSpace( pubPrivFilePath ) && string.IsNullOrWhiteSpace( pubOnlyFilePath ) )
                throw new ArgumentException( "Invalid argument" );

            if( string.IsNullOrWhiteSpace( keyContainerName ) )
                keyContainerName = null;

            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = keyContainerName
            };
            GenerateRsaKeys( cspParams, pubPrivFilePath, pubOnlyFilePath, keySize );
        }
        public static void GenerateRsaKeys(CspParameters cspParams, string pubPrivFilePath, string pubOnlyFilePath, int keySize = 0)
        {
            RSACryptoServiceProvider rsaKey = keySize > 0 ?
                new RSACryptoServiceProvider( keySize, cspParams ) :
                new RSACryptoServiceProvider( cspParams );


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

        public static RSACryptoServiceProvider LoadRsaKeys(string keyContainerName = null, string filePath = null, CspProviderFlags flags = CspProviderFlags.NoFlags)
        {
            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = keyContainerName,
                Flags = flags
            };

            return LoadRsaKeys( cspParams, !string.IsNullOrWhiteSpace( keyContainerName ) ? null : filePath );
        }

        public static RSACryptoServiceProvider LoadRsaKeys(CspParameters cspParams, string filePath)
        {
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );

            if( string.IsNullOrWhiteSpace( filePath ) )
                return rsaKey;
            else
            {
                try
                {
                    Uri uri = new Uri( filePath );
                    string uriContent = WebRequestClient.GetString( uri.ToString() );
                    rsaKey.FromXmlString( uriContent );
                }
                catch
                {
                    try
                    {
                        using( StreamReader sr = new StreamReader( filePath ) )
                            rsaKey.FromXmlString( sr.ReadToEnd() );
                    }
                    catch(Exception innerEx)
                    {
                        throw new FileNotFoundException( $"Could not load RSA keys from [{filePath}].", innerEx );
                    }
                }

                return rsaKey;
            }
        }
        #endregion


        #region Encrypt
        public static string EncryptFromFileKeys(string filePath, string value)
        {
            return Encrypt( filePath: filePath, value: value );
        }

        public static string EncryptFromFileKeys(string filePath, string value, CspProviderFlags flags)
        {
            return Encrypt( filePath: filePath, flags: flags, value: value );
        }

        public static string EncryptFromContainerKeys(string keyContainerName, string value)
        {
            return Encrypt( keyContainerName: keyContainerName, value: value );
        }

        public static string EncryptFromContainerKeys(string keyContainerName, string value, CspProviderFlags flags)
        {
            return Encrypt( keyContainerName: keyContainerName, flags: flags, value: value );
        }

        public static string Encrypt(string keyContainerName = null, string filePath = null, CspProviderFlags flags = CspProviderFlags.NoFlags, string value = null)
        {
            if( string.IsNullOrWhiteSpace( value ) )
                throw new ArgumentException( "Invalid argument" );

            RSACryptoServiceProvider rsa = null;
            if( !string.IsNullOrWhiteSpace( keyContainerName ) )   // container name takes precedence over file
                rsa = LoadRsaKeys( keyContainerName: keyContainerName, flags: flags );
            else if( !string.IsNullOrWhiteSpace( filePath ) )
                rsa = LoadRsaKeys( filePath: filePath, flags: flags );
            else
                throw new ArgumentException( "Missing key container name or path to key file." );

            return Encrypt( rsa, value );
        }

        public static string Encrypt(RSACryptoServiceProvider rsa, string value)
        {
            byte[] valueBytes = Encoding.ASCII.GetBytes( value );
            byte[] encrypted = rsa.Encrypt( valueBytes, false );
            return Convert.ToBase64String( encrypted );
        }

        public static string Encrypt(ICryptoRuntime cryptoRuntime, string value)
        {
            string parmValue = new ParameterInfo { Values = new CryptoParameters { Value = value } }.GetSerializedValues();
            ExecuteResult result = cryptoRuntime.Encrypt( new CryptoStartInfo { Parameters = parmValue } );
            if( result.Status == StatusType.Success )
                return result.ExitData.ToString();
            else
                throw new Exception( result.Message );
        }
        #endregion


        #region Decrypt
        public static string DecryptFromFileKeys(string filePath, string value)
        {
            return Decrypt( filePath: filePath, value: value );
        }

        public static string DecryptFromFileKeys(string filePath, string value, CspProviderFlags flags)
        {
            return Decrypt( filePath: filePath, flags: flags, value: value );
        }

        public static string DecryptFromContainerKeys(string keyContainerName, string value)
        {
            return Decrypt( keyContainerName: keyContainerName, value: value );
        }

        public static string DecryptFromContainerKeys(string keyContainerName, string value, CspProviderFlags flags)
        {
            return Decrypt( keyContainerName: keyContainerName, flags: flags, value: value );
        }

        public static string Decrypt(string keyContainerName = null, string filePath = null, CspProviderFlags flags = CspProviderFlags.NoFlags, string value = null)
        {
            if( string.IsNullOrWhiteSpace( value ) )
                throw new ArgumentException( "Invalid argument" );

            RSACryptoServiceProvider rsa = null;
            // container name takes precedence over file
            if( !string.IsNullOrWhiteSpace( keyContainerName ) )
                // set the UseExistingKey so that rsacryptoserviceprovider doesnt go and generate new keys
                rsa = LoadRsaKeys( keyContainerName: keyContainerName, flags: flags | CspProviderFlags.UseExistingKey );
            else if( !string.IsNullOrWhiteSpace( filePath ) )
                rsa = LoadRsaKeys( filePath: filePath, flags: flags );
            else
                throw new ArgumentException( "Missing key container name or path to key file." );

            return Decrypt( rsa, value );
        }

        public static string Decrypt(RSACryptoServiceProvider rsa, string value)
        {
            byte[] valueBytes = Convert.FromBase64String( value );
            byte[] decrypted = rsa.Decrypt( valueBytes, false );
            return Encoding.ASCII.GetString( decrypted );
        }

        public static string Decrypt(ICryptoRuntime cryptoRuntime, string value)
        {
            string parmValue = new ParameterInfo { Values = new CryptoParameters { Value = value } }.GetSerializedValues();
            ExecuteResult result = cryptoRuntime.Decrypt( new CryptoStartInfo { Parameters = parmValue } );
            if( result.Status == StatusType.Success )
                return result.ExitData.ToString();
            else
                throw new Exception( result.Message );
        }
        #endregion


        public static bool KeyContainerExists(string keyContainerName)
        {
            CspParameters cspParams = new CspParameters
            {
                Flags = CspProviderFlags.UseExistingKey,
                KeyContainerName = keyContainerName
            };

            try
            {
                new RSACryptoServiceProvider( cspParams );
                return true;
            }
            catch
            {
                return false;
            }

        }
        #endregion
    }
}