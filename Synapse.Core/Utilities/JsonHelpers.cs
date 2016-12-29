using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core.Utilities
{
    public class JsonHelpers
    {
        //taken from accepted answer on
        //  http://stackoverflow.com/questions/4580397/json-formatter-in-c
        //minor code cleanup, plus added --> s = s.Replace( " \"", "\"" ).Replace( ": {", ":{" );
        //to deal with output of yaml.net

        private const string __indent = "  ";
        public static string FormatJson(string s)
        {
            s = s.Replace( " \"", "\"" ).Replace( ": {", ":{" );
            int indent = 0;
            bool quoted = false;
            StringBuilder sb = new StringBuilder();

            for( int i = 0; i < s.Length; i++ )
            {
                var ch = s[i];
                switch( ch )
                {
                    case '{':
                    case '[':
                    {
                        sb.Append( ch );
                        if( !quoted )
                        {
                            sb.AppendLine();
                            Enumerable.Range( 0, ++indent ).ForEach( item => sb.Append( __indent ) );
                        }
                        break;
                    }

                    case '}':
                    case ']':
                    {
                        if( !quoted )
                        {
                            sb.AppendLine();
                            Enumerable.Range( 0, --indent ).ForEach( item => sb.Append( __indent ) );
                        }
                        sb.Append( ch );
                        break;
                    }

                    case '"':
                    {
                        sb.Append( ch );
                        bool escaped = false;

                        int index = i;
                        while( index > 0 && s[--index] == '\\' )
                            escaped = !escaped;

                        if( !escaped )
                            quoted = !quoted;
                        break;
                    }

                    case ',':
                    {
                        sb.Append( ch );
                        if( !quoted )
                        {
                            sb.AppendLine();
                            Enumerable.Range( 0, indent ).ForEach( item => sb.Append( __indent ) );
                        }
                        break;
                    }

                    case ':':
                    {
                        sb.Append( ch );
                        if( !quoted )
                            sb.Append( " " );
                        break;
                    }

                    default:
                    {
                        sb.Append( ch );
                        break;
                    }
                }
            }
            return sb.ToString();
        }
    }

    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach( var i in ie )
            {
                action( i );
            }
        }
    }
}