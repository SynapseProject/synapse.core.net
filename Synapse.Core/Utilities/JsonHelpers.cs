using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Synapse.Core.Utilities
{
    public class JsonHelpers
    {
        //taken from accepted answer on
        //  http://stackoverflow.com/questions/4580397/json-formatter-in-c
        //minor code cleanup, added ignore of all whitespace not in quotes
        //to deal with output of yaml.net.  Modified to use TextWriters as well.

        private const string __indent = "  ";
        public static string FormatJson(string s)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader reader = new StringReader(s))
            {
                using (StringWriter writer = new StringWriter(sb))
                {
                    FormatJson(reader, writer);
                    writer.Flush();
                }
            }
            return sb.ToString();
        }

        public static void FormatJson(TextReader reader, TextWriter writer)
        {
            int indent = 0;
            bool quoted = false;
            bool escaped = false;

            int  chInt = 0;

            while ( (chInt = reader.Read() ) != -1)
            {
                char ch = (char)chInt;
                switch( ch )
                {
                    case '{':
                    case '[':
                    {
                        writer.Write( ch );
                        if( !quoted )
                        {
                            writer.WriteLine();
                            Enumerable.Range( 0, ++indent ).ForEach( item => writer.Write( __indent ) );
                        }
                        break;
                    }

                    case '}':
                    case ']':
                    {
                        if ( !quoted )
                        {
                            writer.WriteLine();
                            Enumerable.Range( 0, --indent ).ForEach( item => writer.Write( __indent ) );
                        }
                        writer.Write( ch );
                        break;
                    }

                    case '"':
                    {
                        writer.Write( ch );
                        if( !escaped )
                            quoted = !quoted;
                        break;
                    }

                    case '\\':
                    {
                        // Keep Track Of Escapes To Determine If Applies To Double-Quote or Another Back-Slash
                        // "Key" : "Value\\"            (Escape Applies To Back-Slash)
                        // "Key" : " \"Quoted\" Value"  (Escape Applies To Double-Quote)
                        escaped = !escaped;
                        writer.Write(ch);
                        break;
                    }

                    case ',':
                    {
                        writer.Write( ch );
                        if( !quoted )
                        {
                            writer.WriteLine();
                            Enumerable.Range( 0, indent ).ForEach( item => writer.Write( __indent ) );
                        }
                        break;
                    }

                    case ':':
                    {
                        writer.Write( ch );
                        if( !quoted )
                            writer.Write( " " );
                        break;
                    }

                    default:
                    {
                        // Ignore Any White Space Characters That Aren't Between Quotes
                        if (!Char.IsWhiteSpace(ch) || quoted)
                            writer.Write( ch );
                        break;
                    }
                }

                if (ch != '\\')
                    escaped = false;
            }
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