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
            Dictionary<object, object> p = ConvertDynamicValuesToDict( patch, values );
            ApplyPatchValues( source, p );
        }

        static Dictionary<object, object> ConvertDynamicValuesToDict(List<DynamicValue> patch, Dictionary<string, string> values)
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


        #region foreach
        public static List<string> ExpandForEachAndApplyPatchValues(ref Dictionary<object, object> source, List<ForEach> forEach)
        {
            ForEach node = forEach[0];
            for( int i = 1; i < forEach.Count; i++ )
            {
                node.Child = forEach[i];
                node = forEach[i];
            }

            List<Dictionary<object, object>> matrix = new List<Dictionary<object, object>>();
            ExpandMatrixApplyPatchValues( forEach[0], source, matrix );

            List<string> patchedParms = new List<string>();
            foreach( Dictionary<object, object> parms in matrix )
                patchedParms.Add( Serialize( parms ) );

            return patchedParms;
        }

        static void ExpandMatrixApplyPatchValues(ForEach fe, Dictionary<object, object> source, List<Dictionary<object, object>> matrix)
        {
            foreach( string v in fe.Values )
            {
                ConvertForEachValuesToDict( ref fe, v );
                ApplyPatchValues( source, fe.PathAsPatchValues );
                if( fe.HasChild )
                    ExpandMatrixApplyPatchValues( fe.Child, source, matrix );
                else
                    matrix.Add( CopyDictionary( source ) );
            }
        }

        static void ConvertForEachValuesToDict(ref ForEach fe, string value)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            Dictionary<object, object> d = dict;

            string[] keys = fe.Path.ToString().Split( ':' );
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
            d[keys[lastIndex]] = value;

            fe.PathAsPatchValues = dict;
        }

        static Dictionary<object, object> CopyDictionary(Dictionary<object, object> source)
        {
            Dictionary<object, object> copy = new Dictionary<object, object>();
            foreach( object key in source.Keys )
            {
                copy.Add( key, source[key] );
                if( copy[key] is Dictionary<object, object> )
                {
                    copy[key] = CopyDictionary( (Dictionary<object, object>)copy[key] );
                }
            }

            return copy;
        }
        #endregion
    }
}