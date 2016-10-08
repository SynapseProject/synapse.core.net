using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Core.Utilities
{
    public class CrytoHelpers
    {
        public static string Encode(string value)
        {
            byte[] valueBytes = ASCIIEncoding.ASCII.GetBytes( value );
            return Convert.ToBase64String( valueBytes );
        }

        public static string Decode(string value)
        {
            byte[] valueBytes = Convert.FromBase64String( value );
            return ASCIIEncoding.ASCII.GetString( valueBytes );
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
    }
}