using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Synapse.Common.WebApi
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts the IDictionary to a QueryString
        /// </summary>
        /// <param name="dynamicData">The key/value pairs to convert.</param>
        /// <param name="asPartialQueryString">Indicates whether to prefix the QueryString with a '?' (true) or a '&' (false).</param>
        /// <returns>The IDictionary as a QueryString</returns>
        public static string ToQueryString(this IDictionary<string, string> dynamicData, bool asPartialQueryString = false)
        {
            if( dynamicData == null || dynamicData.Count <= 0 )
                return "";

            StringBuilder qs = new StringBuilder();

            string delim = asPartialQueryString ? "&" : "?";
            foreach( string key in dynamicData.Keys )
            {
                qs.Append( $"{delim}{HttpUtility.UrlEncode( key )}={HttpUtility.UrlEncode( dynamicData[key] )}" );
                delim = "&";
            }

            return qs.ToString();
        }

        /// <summary>
        /// Converts the Uri.Query into a Dictionary
        /// </summary>
        /// <param name="uri">The Uri to convert.</param>
        /// <returns>The Uri.Query as a Dictionary.</returns>
        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString( uri.Query );
            Dictionary<string, string> d = new Dictionary<string, string>( nvc.Count );
            foreach( string key in nvc.AllKeys )
                d[key] = nvc[key];
            return d;
        }
    }
}