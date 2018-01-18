using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
                string value = dynamicData[key];
                //if( value.Contains( ":" ) ) value = value.EncapsulateWith( "'" );
                qs.Append( $"{delim}{HttpUtility.UrlEncode( key )}={HttpUtility.UrlEncode( value )}" );
                delim = "&";
            }

            return qs.ToString();
        }

        //public static void PrepareValuesForPost(this IDictionary<string, string> dynamicData)
        //{
        //    if( dynamicData == null || dynamicData.Count <= 0 )
        //        return;

        //    foreach( string key in dynamicData.Keys.ToArray() )
        //    {
        //        string value = dynamicData[key];
        //        if( value.Contains( ":" ) ) dynamicData[key] = value.EncapsulateWith( "'" );
        //    }
        //}

        /// <summary>
        /// Converts the Uri.Query into a Dictionary
        /// </summary>
        /// <param name="uri">The Uri to convert.</param>
        /// <returns>The Uri.Query as a Dictionary.</returns>
        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString( uri.Query );
            Dictionary<string, string> d = new Dictionary<string, string>( nvc.Count, StringComparer.OrdinalIgnoreCase );
            foreach( string key in nvc.AllKeys )
                d[key] = HttpUtility.UrlDecode( nvc[key] );
            return d;
        }


        //public static string EncapsulateWith(this string s, string c)
        //{
        //    if( !s.StartsWith( c ) ) { s = $"{c}{s}"; }
        //    if( !s.EndsWith( c ) ) { s = $"{s}{c}"; }
        //    return s;
        //}
    }
}