using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Synapse.Core.Utilities
{
    public class YamlHelpers
    {
        static readonly string __sli = "SynapseListIndex";

        #region Serialize/Deserialize
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
        #endregion


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


        #region crypto
        public static Plan EncryptPlan(Plan p)
        {
            Stack<List<ActionItem>> actionLists = new Stack<List<ActionItem>>();
            actionLists.Push( p.Actions );

            while( actionLists.Count > 0 )
            {
                List<ActionItem> actions = actionLists.Pop();
                foreach( ActionItem a in actions )
                {
                    if( a.HasParameters && a.Parameters.HasCrypto )
                    {
                        a.Parameters.Crypto.LoadRsaKeys();
                        a.Parameters.Crypto.IsEncryptMode = true;

                        string p0 = Serialize( a.Parameters );
                        Dictionary<object, object> source = Deserialize( p0 );
                        foreach( string element in a.Parameters.Crypto.Elements )
                        {
                            Dictionary<object, object> patch = ConvertPathElementToDict( element );
                            HandleElementCrypto( source, patch, a.Parameters.Crypto );
                        }
                        p0 = Serialize( source );
                        a.Parameters = Deserialize<ParameterInfo>( p0 );
                    }
                }
            }

            return p;
        }

        static void HandleElementCrypto(Dictionary<object, object> source, Dictionary<object, object> patch, CryptoProvider crypto)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Keys )
            {
                if( source.ContainsKey( key ) )
                {
                    if( source[key] is string )
                        source[key] = crypto.HandleCrypto( source[key].ToString() );
                    else
                    {
                        if( source[key] is Dictionary<object, object> )
                        {
                            if( patch[key] is Dictionary<object, object> )
                                HandleElementCrypto( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key], crypto );
                            else
                                ((Dictionary<object, object>)source[key]).Add( "MergeError", patch[key] );
                            //throw new Exception( $"Dictionary: patch[key] is {(patch[key]).GetType()}" );
                        }
                        else if( source[key] is List<object> )
                        {
                            if( patch[key] is List<object> )
                                HandleElementCrypto( (List<object>)source[key], (List<object>)patch[key], crypto );
                            else
                                ((List<object>)source[key]).Add( patch[key] );
                            //throw new Exception( $"IndexedKey: patch[key] is {(patch[key]).GetType()}" );
                        }
                    }
                }
            }
        }

        static void HandleElementCrypto(List<object> source, List<object> patch, CryptoProvider crypto)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            Dictionary<object, object> patchItem = null;
            if( patch.Count > 0 && patch[0] is Dictionary<object, object> )
                patchItem = (Dictionary<object, object>)patch[0];
            else
                return;

            int i = 0;
            bool ok = patchItem.ContainsKey( __sli ) && int.TryParse( patchItem[__sli].ToString(), out i );
            if( ok && source.Count >= i + 1 )
            {
                patchItem.Remove( __sli );
                object patchKey = null;
                foreach( object key in patchItem.Keys )
                    patchKey = key;

                if( source[i] is Dictionary<object, object> )
                {
                    Dictionary<object, object> currSource = (Dictionary<object, object>)source[i];

                      object patchValue = null;
                    if( ((Dictionary<object, object>)patch[0]).ContainsKey( patchKey ) )
                        patchValue = ((Dictionary<object, object>)patch[0])[patchKey];

                    if( patchValue is Dictionary<object, object> )
                        HandleElementCrypto( (Dictionary<object, object>)currSource[patchKey], (Dictionary<object, object>)patchValue, crypto );
                    else if( patchValue is List<object> )
                        HandleElementCrypto( (List<object>)currSource[patchKey], (List<object>)patchValue, crypto );
                    else if( patchValue == null )
                        ((Dictionary<object, object>)source[i])[patchKey] = crypto.HandleCrypto( (((Dictionary<object, object>)source[i])[patchKey]).ToString() );
                    else
                        ((Dictionary<object, object>)source[i]).Add( "MergeError", patchKey );
                }
                else
                {
                    if( patchKey is List<object> )
                        HandleElementCrypto( (List<object>)source[i], (List<object>)patchKey, crypto );
                    else
                        ((List<object>)source[i]).Add( patchKey );
                }
            }
            //else
            //    source.Add( key );
        }

        public static Dictionary<object, object> EncryptPlan_X(Plan p)
        {
            //convert the Plan instance to a dictionary
            string yaml = p.ToYaml();
            Dictionary<object, object> planDict = Plan.FromYamlAsDictionary( yaml );

            //collect the element paths to encrypt
            Dictionary<object, object> cryptoPaths = ConvertCryptoElementsToDictionary( p.Crypto );

            //encrypt the data
            p.Crypto.LoadRsaKeys();
            p.Crypto.IsEncryptMode = true;
            HandleCrypto( planDict, cryptoPaths, p.Crypto );

            return planDict;
        }

        public static Plan DecryptPlan(Dictionary<object, object> planDict)
        {
            Plan result = new Plan();

            //convert the dictionary-based representation of Crypto into a class instance
            object ocp = planDict[nameof( result.Crypto )];
            string scp = Serialize( ocp );
            CryptoProvider cp = Deserialize<CryptoProvider>( scp );

            //collect the element paths to decrypt
            Dictionary<object, object> cryptoPaths = ConvertCryptoElementsToDictionary( cp );

            //decrypt the data
            cp.LoadRsaKeys();
            cp.IsEncryptMode = false;
            HandleCrypto( planDict, cryptoPaths, cp );

            //convert the dictionary-based representation of Plan into a class instance
            string yaml = Serialize( planDict );
            using( StringReader reader = new StringReader( yaml ) )
                result = Plan.FromYaml( reader );
            return result;
        }

        static Dictionary<object, object> ConvertCryptoElementsToDictionary(CryptoProvider crypto)  //, Dictionary<string, string> values
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            Dictionary<object, object> d = dict;
            foreach( string element in crypto.Elements )
            {
                string[] keys = element.Split( ':' );
                int lastIndex = keys.Length - 1;

                for( int i = 0; i <= lastIndex; i++ )
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

                string xxx = keys[lastIndex];
                if( !d.ContainsKey( xxx ) )
                    d[xxx] = null;

                d = dict;
            }

            return dict;
        }

        static void HandleCrypto(Dictionary<object, object> source, Dictionary<object, object> patch, CryptoProvider crypto)
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
                            HandleCrypto( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key], crypto );
                        else
                            ((Dictionary<object, object>)source[key]).Add( "MergeError", patch[key] );
                        //throw new Exception( $"Dictionary: patch[key] is {(patch[key]).GetType()}" );
                    }
                    else if( source[key] is List<object> )
                    {
                        if( patch[key] is IndexedKey )
                            HandleCrypto( (List<object>)source[key], (IndexedKey)patch[key], crypto );
                        else
                            ((List<object>)source[key]).Add( patch[key] );
                        //throw new Exception( $"IndexedKey: patch[key] is {(patch[key]).GetType()}" );
                    }
                }
                else
                    source[key] = crypto.HandleCrypto( source[key].ToString() );
            }
        }

        static void HandleCrypto(List<object> source, IndexedKey patch, CryptoProvider crypto)
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
                            HandleCrypto( (Dictionary<object, object>)source[i], (Dictionary<object, object>)patch.Values[key], crypto );
                        else
                            ((Dictionary<object, object>)source[i]).Add( "MergeError", patch.Values[key] );
                    }
                    else
                    {
                        if( patch.Values[key] is IndexedKey )
                            HandleCrypto( (List<object>)source[i], (IndexedKey)patch.Values[key], crypto );
                        else
                            ((List<object>)source[i]).Add( patch.Values[key] );
                    }
                }
                else
                    source.Add( patch.Values[key] );
            }
        }
        #endregion


        #region util
        static Dictionary<object, object> ConvertPathElementToDict(string path)
        {
            StringBuilder yaml = new StringBuilder();

            string[] lines = path.Split( ':' );
            for( int i = 0; i < lines.Length; i++ )
            {
                int index = -1;
                string newLine = CheckPathElementIsIndexed( lines[i], out index );
                yaml.AppendLine( newLine.PadLeft( (2 * i) + newLine.Length ) + ":" );

                if( index > -1 )
                {
                    string sli = $"- {__sli}: {index}";
                    yaml.AppendLine( sli.PadLeft( (2 * i) + sli.Length ) );
                }
            }

            return Deserialize( yaml.ToString() );
        }

        static string CheckPathElementIsIndexed(string element, out int index)
        {
            index = -1;
            if( element.EndsWith( "]" ) )
            {
                index = int.Parse( Regex.Match( element, @"\d" ).Value );
                element = element.Substring( 0, element.IndexOf( '[' ) );
            }

            return element;
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