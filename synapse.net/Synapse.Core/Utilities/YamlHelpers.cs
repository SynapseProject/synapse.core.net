using System;
using System.Collections.Generic;
using System.IO;

using YamlDotNet.Serialization;

namespace Synapse.Core.Utilities
{
    public class YamlHelpers
    {
        public static string Serialize(object data)
        {
            string result = null;
            using( StringWriter writer = new StringWriter() )
            {
                Serializer serializer = new Serializer();
                serializer.Serialize( writer, data );
                result = writer.ToString();
            }
            return result;
        }

        public static void Serialize(TextWriter tw, object data)
        {
            Serializer serializer = new Serializer();
            serializer.Serialize( tw, data );
        }

        public static T Deserialize<T>(string yaml, bool ignoreUnmatchedProperties = true)
        {
            using( StringReader reader = new StringReader( yaml ) )
            {
                DeserializerBuilder builder = new DeserializerBuilder();
                if( ignoreUnmatchedProperties )
                    builder.IgnoreUnmatchedProperties();
                Deserializer deserializer = builder.Build();
                return deserializer.Deserialize<T>( reader );
            }
        }

        public static T Deserialize<T>(TextReader reader, bool ignoreUnmatchedProperties = true)
        {
            DeserializerBuilder builder = new DeserializerBuilder();
            if( ignoreUnmatchedProperties )
                builder.IgnoreUnmatchedProperties();
            Deserializer deserializer = builder.Build();
            return deserializer.Deserialize<T>( reader );
        }


        #region merge for yaml/json
        public static Dictionary<object, object> Deserialize(string source)
        {
            return Deserialize<Dictionary<object, object>>( source );
        }

        public static void Merge(ref Dictionary<object, object> source, Dictionary<object, object> patch)
        {
            ApplyPatchValues( source, patch );
        }

        public static void Merge(ref Dictionary<object, object> source, List<DynamicValue> patch, Dictionary<string, string> values)
        {
            Dictionary<object, object> p = ConvertDynamicValuestoDict( patch, values );
            ApplyPatchValues( source, p );
        }

        static Dictionary<object, object> ConvertDynamicValuestoDict(List<DynamicValue> patch, Dictionary<string, string> values)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            Dictionary<object, object> d = dict;
            foreach( DynamicValue v in patch )
            {
                string[] keys = v.Path.ToString().Split( ':' );
                int lastIndex = keys.Length - 1;
                for( int i = 0; i < lastIndex; i++ )
                {
                    string key = keys[i];
                    if( !d.ContainsKey( key ) )
                    {
                        d[key] = new Dictionary<object, object>();
                    }
                    d = (Dictionary<object, object>)d[key];
                }
                if( values.ContainsKey( v.Name ) )
                {
                    d[keys[lastIndex]] = values[v.Name];
                }
            }

            return dict;
        }

        static void ApplyPatchValues(Dictionary<object, object> source, Dictionary<object, object> patch)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Keys )
            {
                if( source.ContainsKey( key ) && patch[key] is Dictionary<object, object> )
                    ApplyPatchValues( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key] );
                else
                    source[key] = patch[key];
            }
        }
        #endregion

    }
}