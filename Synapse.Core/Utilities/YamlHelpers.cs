﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Synapse.Core.Utilities
{
    public class YamlHelpers
    {
        public static void Serialize(TextWriter tw, object data, bool serializeAsJson = false, bool emitDefaultValues = false)
        {
            Serializer serializer = null;
            SerializerBuilder builder = new SerializerBuilder();

            if( serializeAsJson )
                builder.JsonCompatible();

            if( emitDefaultValues )
                builder.EmitDefaults();

            serializer = builder.Build();

            serializer.Serialize( tw, data );
        }

        public static string Serialize(object data, bool serializeAsJson = false, bool emitDefaultValues = false)
        {
            string result = null;

            if( !string.IsNullOrWhiteSpace( data?.ToString() ) )
                using( StringWriter writer = new StringWriter() )
                {
                    Serialize( writer, data, serializeAsJson, emitDefaultValues );
                    result = serializeAsJson ? JsonHelpers.FormatJson( writer.ToString() ) : writer.ToString();
                }

            return result;
        }

        public static void SerializeFile(string path, object data, bool serializeAsJson = false, bool emitDefaultValues = false)
        {
            if( !serializeAsJson )
            {
                using( StreamWriter writer = new StreamWriter( path ) )
                    Serialize( writer, data, serializeAsJson, emitDefaultValues );
            }
            else //gets formatted json
            {
                string result = Serialize( data, serializeAsJson, emitDefaultValues );
                File.WriteAllText( path, result );
            }
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

        public static T DeserializeFile<T>(string path, bool ignoreUnmatchedProperties = true) where T : class
        {
            T ssc = null;
            using( StreamReader reader = new StreamReader( path ) )
            {
                DeserializerBuilder builder = new DeserializerBuilder();
                if( ignoreUnmatchedProperties )
                    builder.IgnoreUnmatchedProperties();
                Deserializer deserializer = builder.Build();
                ssc = deserializer.Deserialize<T>( reader );
            }
            return ssc;
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
            //if there's nothing to do, get out!
            //if( values == null || (values != null && values.Count == 0) )
            //    return;

            Dictionary<object, object> p = ConvertDynamicValuesToDictionary( patch, values );

            ApplyPatchValues( source, p );
        }

        static Dictionary<object, object> ConvertDynamicValuesToDictionary(List<DynamicValue> patch, Dictionary<string, string> values)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            Dictionary<object, object> d = dict;
            foreach( DynamicValue v in patch )
            {
                string[] keys = v.Path.ToString().Split( ':' );
                int lastIndex = keys.Length - 1;

                for( int i = 0; i < lastIndex; i++ )
                {
                    IndexedKey pk = new IndexedKey( keys[i] );

                    if( !d.ContainsKey( pk.Key ) )
                    {
                        if( pk.IsIndexed )
                        {
                            pk.Values = new Dictionary<object, object>();
                            pk.Values[pk.Index] = new Dictionary<object, object>();
                            d[pk.Key] = pk;
                            d = (Dictionary<object, object>)pk.Values[pk.Index];
                        }
                        else
                        {
                            d[pk.Key] = new Dictionary<object, object>();
                            d = (Dictionary<object, object>)d[pk.Key];
                        }
                    }
                    else
                    {
                        if( d[pk.Key] is IndexedKey )
                        {
                            d = ((IndexedKey)d[pk.Key]).Values;
                            if( pk.IsIndexed )
                            {
                                d[pk.Index] = new Dictionary<object, object>();
                                d = (Dictionary<object, object>)d[pk.Index];
                            }
                        }
                        else
                            d = (Dictionary<object, object>)d[pk.Key];
                    }
                }

                if( values.ContainsKey( v.Name ) )
                    d[keys[lastIndex]] = values[v.Name];

                d = dict;
            }

            return dict;
        }

        static void ApplyPatchValues(Dictionary<object, object> source, Dictionary<object, object> patch)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Keys )
            {
                if( source.ContainsKey( key ) && !(source[key] is string) )
                {
                    if( source[key] is Dictionary<object, object> )
                    {
                        if( patch[key] is Dictionary<object, object> )
                            ApplyPatchValues( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key] );
                        else
                            ((Dictionary<object, object>)source[key]).Add( "MergeError", patch[key] );
                            //throw new Exception( $"Dictionary: patch[key] is {(patch[key]).GetType()}" );
                    }
                    else if( source[key] is List<object> )
                    {
                        if( patch[key] is IndexedKey )
                            ApplyPatchValues( (List<object>)source[key], (IndexedKey)patch[key] );
                        else
                            ((List<object>)source[key]).Add( patch[key] );
                            //throw new Exception( $"IndexedKey: patch[key] is {(patch[key]).GetType()}" );
                    }
                }
                else
                    source[key] = patch[key];
            }
        }

        static void ApplyPatchValues(List<object> source, IndexedKey patch)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Values.Keys )
            {
                int i = 0;
                bool ok = int.TryParse( key.ToString(), out i );
                if( ok && source.Count >= i + 1 )
                {
                    if( source[i] is Dictionary<object, object> )
                    {
                        if( patch.Values[key] is Dictionary<object, object> )
                            ApplyPatchValues( (Dictionary<object, object>)source[i], (Dictionary<object, object>)patch.Values[key] );
                        else
                            ((Dictionary<object, object>)source[i]).Add( "MergeError", patch.Values[key] );
                    }
                    else
                    {
                        if( patch.Values[key] is IndexedKey )
                            ApplyPatchValues( (List<object>)source[i], (IndexedKey)patch.Values[key] );
                        else
                            ((List<object>)source[i]).Add( patch.Values[key] );
                    }
                }
                else
                    source.Add( patch.Values[key] );
            }
        }
        #endregion


        #region foreach
        public static List<object> ExpandForEachAndApplyPatchValues(ref Dictionary<object, object> source, List<ForEach> forEach)
        {
            ForEach node = forEach[0];
            for( int i = 1; i < forEach.Count; i++ )
            {
                node.Child = forEach[i];
                node = forEach[i];
            }

            List<Dictionary<object, object>> matrix = new List<Dictionary<object, object>>();
            ExpandMatrixApplyPatchValues( forEach[0], source, matrix );

            return new List<object>( matrix );
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
                    d[key] = new Dictionary<object, object>();

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
                    copy[key] = CopyDictionary( (Dictionary<object, object>)copy[key] );
            }

            return copy;
        }
        #endregion
    }


    class IndexedKey
    {
        public IndexedKey(object key)
        {
            string k = key.ToString();
            int index = -1;
            if( k.EndsWith( "]" ) )
            {
                index = int.Parse( Regex.Match( k, @"\d" ).Value );
                k = k.Substring( 0, k.IndexOf( '[' ) );
            }

            Key = k;
            if( index > -1 )
                Index = index;
        }

        public object Key { get; }
        [YamlIgnore]
        public int? Index { get; set; }
        public bool IsIndexed { get { return Index.HasValue; } }

        public Dictionary<object, object> Values { get; set; }
    }
}