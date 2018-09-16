﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Synapse.Core.Utilities
{
    public class YamlHelpers
    {
        static readonly string __sli = "SynapseListIndex";
        static readonly string __token = "__synapse__token__";

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

        public static string Serialize(object data, bool serializeAsJson = false, bool formatJson = true, bool emitDefaultValues = false)
        {
            string result = null;

            if( !string.IsNullOrWhiteSpace( data?.ToString() ) )
                using( StringWriter writer = new StringWriter() )
                {
                    Serialize( writer, data, serializeAsJson, emitDefaultValues );
                    result = serializeAsJson && formatJson ? JsonHelpers.FormatJson( writer.ToString() ) : writer.ToString();
                }

            return result;
        }

        public static void SerializeFile(string path, object data, bool serializeAsJson = false, bool formatJson = true, bool emitDefaultValues = false)
        {
            using( StreamWriter writer = new StreamWriter( path ) )
                Serialize( writer, data, serializeAsJson, emitDefaultValues );

            if( formatJson && serializeAsJson )
            {
                String origFile = $"{path}.tmp";
                File.Move( path, origFile );
                using( StreamReader raw = new StreamReader( origFile ) )
                using( StreamWriter formatted = new StreamWriter( path ) )
                    JsonHelpers.FormatJson( raw, formatted );

                File.Delete( origFile );
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

        public static Dictionary<object, object> Deserialize(string source)
        {
            return Deserialize<Dictionary<object, object>>( source );
        }
        #endregion


        #region merge for yaml/json
        public static void Merge(ref Dictionary<object, object> source, Dictionary<object, object> patch)
        {
            ApplyPatchValues( source, patch );
        }

        public static void Merge(ref Dictionary<object, object> source, List<DynamicValue> dynamicValues, Dictionary<string, string> values)
        {
            //if there's nothing to do, get out!
            if( dynamicValues == null || dynamicValues?.Count == 0 )
                return;

            foreach( DynamicValue dv in dynamicValues )
            {
                string val = null;
                if( (values?.ContainsKey( dv.Source )).Value && !string.IsNullOrWhiteSpace( values[dv.Source] ) )
                    val = values[dv.Source];

                if( string.IsNullOrWhiteSpace( val ) && dv.TryGetDefaultValue( out object defaultValue ) )
                    if( defaultValue != null )
                        val = Serialize( defaultValue );

                if( !string.IsNullOrWhiteSpace( val ) || dv.DefaultAllowNull )
                {
                    if( !dv.Validate( val, out string validationErrorMessage ) )
                        throw new ArgumentException( validationErrorMessage );

                    object value = dv.Parse ? TryParseValue( val ) : val;
                    Dictionary<object, object> patch = ConvertPathElementToDict( dv.Target, value );
                    ApplyPatchValues( source, patch, dv );
                }
            }
        }

        public static void Merge(ref Dictionary<object, object> destination, List<ParentExitDataValue> parentExitData, ref Dictionary<object, object> parentExitDataValues)
        {
            //if there's nothing to do, get out!
            if( parentExitDataValues == null || (parentExitDataValues != null && parentExitDataValues.Count == 0) )
                return;

            IEnumerable<ParentExitDataValue> transforms = parentExitData.Where( pex => pex.HasTransformInPlace );
            foreach( ParentExitDataValue ped in transforms )
            {
                string[] path = ped.Source?.Split( ':' );
                if( path.Length > 0 )
                {
                    object element = null;
                    try
                    {
                        element = SelectElements( parentExitDataValues, new List<string>() { ped.TransformInPlace.Source } );
                    }
                    catch( Exception ex )
                    {
                        throw new Exception( $"Could not resolve ParentExitData->TransformInPlace->Source: [{ped.TransformInPlace.Source}] for Target: [{ped.TransformInPlace.Target}].  Exception: {ex.Message}" );
                    }

                    if( element != null )
                    {
                        if( ped.TransformInPlace.Parse )
                            element = TryParseValue( element );
                        Dictionary<object, object> patch = ConvertPathElementToDict( ped.TransformInPlace.Target, element ); //?.ToString()
                        ApplyPatchValues( parentExitDataValues, patch, ped.TransformInPlace );
                    }
                }
            }

            foreach( ParentExitDataValue ped in parentExitData )
            {
                string[] path = ped.Source?.Split( ':' );
                if( path?.Length > 0 )
                {
                    object element = null;
                    try
                    {
                        element = SelectElements( parentExitDataValues, new List<string>() { ped.Source } );
                    }
                    catch( Exception ex )
                    {
                        throw new Exception( $"Could not resolve ParentExitData->CopyToValues->Source: [{ped.Source}] for Target: [{ped.CopyToValues.Target}].  Exception: {ex.Message}" );
                    }

                    if( element != null )
                    {
                        if( ped.CopyToValues.Parse )
                            element = TryParseValue( element );

                        Dictionary<object, object> patch = ConvertPathElementToDict( ped.CopyToValues.Target, element ); //?.ToString()
                        ApplyPatchValues( destination, patch, ped.CopyToValues );
                    }
                }
            }
        }

        internal static void SelectForEachFromValues(List<ForEachItem> forEachFromValues, ref Dictionary<object, object> values,
            Dictionary<string, ParameterInfo> globalParamSets, object parentExitData)
        {
            foreach( ForEachItem fe in forEachFromValues )
            {
                ParameterSourceInfo pss = fe.ParameterSource;

                string[] path = pss.Source?.Split( ':' );
                if( path.Length > 0 )
                {
                    Dictionary<object, object> parms = pss.HasName ? GetParamSet( pss.Name, globalParamSets, pss.IsNameParentExitData, parentExitData ) : values;

                    object element = null;
                    try
                    {
                        element = SelectElements( parms, new List<string>() { pss.Source } );
                    }
                    catch( Exception ex )
                    {
                        throw new Exception( $"Could not resolve ForEach->ParameterSourceItems->Source: [{pss.Source}].  Exception: {ex.Message}" );
                    }


                    if( element != null )
                    {
                        if( pss.Parse )
                            element = TryParseValue( element );

                        if( element is List<object> )
                            fe.Values = (List<object>)element;
                        else
                        {
                            string elementAsYaml = Serialize( element );
                            Dictionary<object, object> elementAsDict = Deserialize( elementAsYaml );
                            fe.Values = FindFirstListInDict( elementAsDict );
                        }
                    }
                }
            }
        }

        static Dictionary<object, object> GetParamSet(string key, Dictionary<string, ParameterInfo> globalParamSets, bool isNameParentExitData, object parentExitData)
        {
            Dictionary<object, object> p = new Dictionary<object, object>();

            if( isNameParentExitData )
            {
                string values = Serialize( parentExitData );
                p = Deserialize( values );
            }
            else if( globalParamSets.ContainsKey( key ) )
            {
                string values = Serialize( globalParamSets[key].Values );
                p = Deserialize( values );
            }

            return p;
        }

        static List<object> FindFirstListInDict(Dictionary<object, object> dict)
        {
            List<object> list = null;

            foreach( object key in dict.Keys )
            {
                object item = dict[key];

                if( item is Dictionary<object, object> )
                    list = FindFirstListInDict( (Dictionary<object, object>)item );
                else if( item is List<object> )
                    list = (List<object>)item;

                if( list != null )
                    break;
            }

            return list;
        }

        internal static object TryParseValue(object value)
        {
            if( value is string )
                try
                {
                    string s = value as string;
                    if( !string.IsNullOrWhiteSpace( s ) )
                        if( s.StartsWith( "[" ) ) //it's a json array, which doesn't parse,
                            value = Deserialize( $"{__token}: {s}" )[__token]; //so make it yaml, then de-yaml it
                        else if( s.StartsWith( "'" ) && s.EndsWith( "'" ) )
                            value = Deserialize( s.Substring( 1, s.Length - 2 ) ); //it's '-quote encapsulated
                        else
                            value = Deserialize( s );
                }
                catch( Exception ex ) { string e = ex.Message; }//eat the error

            return value;
        }

        /// <summary>
        /// Downward recurse *patch* Dicts/Lists to the bottom**, then set value in source.
        /// **Bottom: Each patch[key] must exist as source[key], or all of patch[key] will be added at source[key]
        /// </summary>
        /// <param name="source">The destination dict into which patch will be merged.</param>
        /// <param name="patch">The dict to merge in.</param>
        /// <param name="dv">Optional DynamicValue for specifying Regex/Crypto options of destinatio value.</param>
        internal static void ApplyPatchValues(Dictionary<object, object> source, Dictionary<object, object> patch, IReplacementValueOptions dv = null)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Keys )
            {
                if( source.ContainsKey( key ) )
                {
                    if( source[key] is Dictionary<object, object> )
                        ApplyPatchValues( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key], dv );
                    else if( source[key] is List<object> )
                        ApplyPatchValues( (List<object>)source[key], (List<object>)patch[key], dv );
                    else //( source[key] is string )
                    {
                        if( patch[key] is Dictionary<object, object> )
                        {
                            source[key] = new Dictionary<object, object>();
                            ApplyPatchValues( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key], dv );
                        }
                        else if( patch[key] is List<object> )
                        {
                            source[key] = new List<object>();
                            ApplyPatchValues( (List<object>)source[key], (List<object>)patch[key], dv );
                        }
                        else
                            source[key] = RegexReplaceOrValue( source[key], patch[key], dv );
                    }
                }
                else
                {
                    if( patch[key] is Dictionary<object, object> )
                    {
                        source[key] = new Dictionary<object, object>();
                        ApplyPatchValues( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key], dv );
                    }
                    else if( patch[key] is List<object> )
                    {
                        source[key] = new List<object>();
                        ApplyPatchValues( (List<object>)source[key], (List<object>)patch[key], dv );
                    }
                    else
                        source[key] = patch[key];
                }
            }
        }

        /// <summary>
        /// Examines (patch[0] is Dictionary<object, object>):
        ///     true:  remove metadata of indexValue (__sli) and continue recursion of objects
        ///     false: Clear source list, put patch values in: [source.Clear(); source.AddRange( patch );]
        /// </summary>
        /// <param name="source">The destination dict into which patch will be merged.</param>
        /// <param name="patch">The dict to merge in.</param>
        /// <param name="dv">Optional DynamicValue for specifying Regex/Crypto options of destinatio value.</param>
        internal static void ApplyPatchValues(List<object> source, List<object> patch, IReplacementValueOptions dv = null)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            //updated syntax to Pattern Matching: if patch[0] is Dict => patchItem
            if( patch?.Count > 0 && patch[0] is Dictionary<object, object> patchItem && patchItem.ContainsKey( __sli ) )
            {
                //get the metadata index value, remove metadata
                int i = int.Parse( patchItem[__sli].ToString() );
                patchItem.Remove( __sli );
                //get the value to updated/added at index 'i'
                object patchKey = patchItem.ElementAt( 0 ).Key;
                object patchValue = patchItem[patchKey];


                if( i >= 0 && i < source.Count )
                {
                    //updated syntax to Pattern Matching: if source[i] is Dict => listItemValue
                    if( source[i] is Dictionary<object, object> listItemValue )
                    {
                        if( patchValue is Dictionary<object, object> )
                        {
                            if( listItemValue[patchKey] == null )
                                listItemValue[patchKey] = new Dictionary<object, object>();
                            ApplyPatchValues( (Dictionary<object, object>)listItemValue[patchKey], (Dictionary<object, object>)patchValue, dv );
                        }
                        else if( patchValue is List<object> )
                        {
                            if( !listItemValue.ContainsKey( patchKey ) )
                                listItemValue.Add( patchKey, new List<object>() );

                            if( listItemValue[patchKey] is List<object> )
                                ApplyPatchValues( (List<object>)listItemValue[patchKey], (List<object>)patchValue, dv );
                        }
                        else //if( patchValue is 'the value' )
                            listItemValue[patchKey] = RegexReplaceOrValue( listItemValue[patchKey], patchValue, dv );
                    }
                    else if( source[i] is List<object> )
                        //recurse back in
                        ApplyPatchValues( (List<object>)source[i], (List<object>)patchKey, dv );
                    else
                        //update the value at index i
                        source[i] = RegexReplaceOrValue( source[i], patchValue, dv );
                }
                else //i is outside source lbound/ubound
                {
                    if( patchValue is Dictionary<object, object> )
                    {
                        source.Add( new Dictionary<object, object>() { { patchKey, new Dictionary<object, object>() } } );
                        ApplyPatchValues( (Dictionary<object, object>)((Dictionary<object, object>)source[source.Count - 1])[patchKey],
                            (Dictionary<object, object>)patchValue, dv );
                    }
                    else if( patchValue is List<object> )
                    {
                        source.Add( new Dictionary<object, object>() { { patchKey, new List<object>() } } );
                        ApplyPatchValues( (Dictionary<object, object>)source[source.Count - 1], patchItem, dv );
                    }
                    else
                        //add the value to the collection, where [string.empty] is a placeholder for the non-existent value at index i
                        source.Add( RegexReplaceOrValue( string.Empty, patchValue, dv ) );
                }
            }
            else
            {
                //the patch value is a List (not a Dict), so replace the whole List at the destination
                source.Clear();
                source.AddRange( patch );
            }
        }

        internal static object RegexReplaceOrValue(object input, object replacement, IReplacementValueOptions rv)
        {
            object value = replacement;

            if( rv != null )
            {
                if( rv.HasReplace )
                {
                    string repl = $"{replacement}";
                    if( rv.IsBase64Encode )
                        repl = CryptoHelpers.Encode( repl );
                    value = Regex.Replace( input.ToString(), rv.Replace, repl, RegexOptions.IgnoreCase );
                }
                else if( rv.IsBase64Encode )
                    value = CryptoHelpers.Encode( $"{value}" );
            }

            return value;
        }
        #endregion


        #region foreach
        public static List<object> ExpandForEachAndApplyPatchValues(ref Dictionary<object, object> source, List<ForEachItem> forEach)
        {
            ForEachItem node = forEach[0];
            for( int i = 1; i < forEach.Count; i++ )
            {
                node.Child = forEach[i];
                node = forEach[i];
            }

            List<Dictionary<object, object>> matrix = new List<Dictionary<object, object>>();
            ExpandMatrixApplyPatchValues( forEach[0], source, matrix );

            return new List<object>( matrix );
        }

        static void ExpandMatrixApplyPatchValues(ForEachItem fe, Dictionary<object, object> source, List<Dictionary<object, object>> matrix)
        {
            foreach( object v in fe.Values )
            {
                fe.PathAsPatchValues = ConvertPathElementToDict( fe.Target, v );
                ApplyPatchValues( source, fe.PathAsPatchValues, fe );
                if( fe.HasChild )
                    ExpandMatrixApplyPatchValues( fe.Child, source, matrix );
                else
                {
                    //make a copy of the Dictionary to avoid object refs
                    string yaml = Serialize( source );
                    Dictionary<object, object> copy = Deserialize( yaml );
                    matrix.Add( copy );
                }
            }
        }
        #endregion


        #region crypto
        public static T GetCryptoValues<T>(T c, bool isEncryptMode = true) where T : class, ICrypto
        {
            T result = null;

            if( c.HasCrypto )
            {
                c.Crypto.LoadRsaKeys();
                c.Crypto.IsEncryptMode = isEncryptMode;

                List<string> errors = new List<string>();
                string p = Serialize( c );
                Dictionary<object, object> source = Deserialize( p );
                foreach( string element in c.Crypto.Elements )
                {
                    try
                    {
                        Dictionary<object, object> patch = ConvertPathElementToDict( element );
                        HandleElementCrypto( source, patch, c.Crypto );
                    }
                    catch( Exception ex )
                    {
                        errors.Add( $"Element: [{element}] failed with error: [{ex.Message.Trim()}]." );
                    }
                }

                p = Serialize( source );
                result = Deserialize<T>( p );

                if( errors.Count == 0 )
                    result.Crypto.Errors = null;
                else
                    foreach( string error in errors )
                        result.Crypto.Errors.Add( error );
            }

            return result;
        }

        public static Plan HandlePlanCrypto(Plan p, bool isEncryptMode = true)
        {
            if( p.HasRunAs && p.RunAs.HasCrypto )
                p.RunAs = p.RunAs.GetCryptoValues( p.RunAs.Crypto, isEncryptMode );

            Stack<IEnumerable<ActionItem>> actionLists = new Stack<IEnumerable<ActionItem>>();
            actionLists.Push( p.Actions );

            while( actionLists.Count > 0 )
            {
                IEnumerable<ActionItem> actions = actionLists.Pop();

                foreach( ActionItem a in actions )
                {
                    if( a.HasRunAs && a.RunAs.HasCrypto )
                        a.RunAs = a.RunAs.GetCryptoValues( p.Crypto, isEncryptMode );
                    if( a.Handler != null && a.Handler.HasConfig && a.Handler.Config.HasCrypto )
                        a.Handler.Config = a.Handler.Config.GetCryptoValues( p.Crypto, isEncryptMode );
                    if( a.HasParameters && a.Parameters.HasCrypto )
                        a.Parameters = a.Parameters.GetCryptoValues( p.Crypto, isEncryptMode );

                    if( a.HasActionGroup )
                        actionLists.Push( new ActionItem[] { a.ActionGroup } );
                    if( a.HasActions )
                        actionLists.Push( a.Actions );
                }
            }

            return p;
        }

        //note: [ss]: I removed all the error handling out of this (and List overload below), so if they fail
        //            they'll just throw an Exception to be caught by the topmost try/catch, where the path is logged as in-error.
        internal static void HandleElementCrypto(Dictionary<object, object> source, Dictionary<object, object> patch, CryptoProvider crypto)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Keys )
            {
                if( source[key] is Dictionary<object, object> )
                    HandleElementCrypto( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key], crypto );
                else if( source[key] is List<object> )
                    HandleElementCrypto( (List<object>)source[key], (List<object>)patch[key], crypto );
                else //( source[key] is string )
                    source[key] = crypto.SafeHandleCrypto( source[key].ToString() );
            }
        }

        internal static void HandleElementCrypto(List<object> source, List<object> patch, CryptoProvider crypto)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            Dictionary<object, object> patchItem = (Dictionary<object, object>)patch[0];

            int i = int.Parse( patchItem[__sli].ToString() );
            patchItem.Remove( __sli );

            object patchKey = null;
            object patchValue = null;

            //todo: [ss]: from here, to..
            foreach( object key in patchItem.Keys )
                patchKey = key;

            if( patchItem.ContainsKey( patchKey ) )
                patchValue = patchItem[patchKey];
            //..here needs a little examination.  seems redundant to check for a key in the collection
            //that was just iterated above.  further, what is the use-case for iterating down after
            //removing [__sli]?  When is there a third item in the collection?

            if( i >= 0 && i < source.Count )
            {
                //updated syntax to Pattern Matching: if source[i] is Dict => listItemValue
                if( source[i] is Dictionary<object, object> listItemValue )
                {
                    if( patchValue is Dictionary<object, object> )
                        HandleElementCrypto( (Dictionary<object, object>)listItemValue[patchKey], (Dictionary<object, object>)patchValue, crypto );
                    else if( patchValue is List<object> )
                        HandleElementCrypto( (List<object>)listItemValue[patchKey], (List<object>)patchValue, crypto );
                    else //if( patchValue == null )
                        listItemValue[patchKey] = crypto.SafeHandleCrypto( ((listItemValue)[patchKey]).ToString() );
                }
                else if( source[i] is List<object> )
                    HandleElementCrypto( (List<object>)source[i], (List<object>)patchKey, crypto );
                else
                    source[i] = crypto.SafeHandleCrypto( source[i].ToString() );
            }
        }
        #endregion


        #region select yaml/json node from doc
        public static object SelectElements(Plan plan, IEnumerable<string> elementPaths)
        {
            Dictionary<object, object> source = Deserialize( plan.ToYaml() );
            return SelectElements( source, elementPaths );
        }

        public static object SelectElements(Dictionary<object, object> source, IEnumerable<string> elementPaths)
        {
            List<object> returnSet = new List<object>();
            foreach( string elementPath in elementPaths )
            {
                Dictionary<object, object> searchPath = ConvertPathElementToDict( elementPath );
                returnSet.Add( FindElement( source, searchPath ) );
            }

            if( returnSet.Count == 1 )
                return returnSet[0];
            else
                return returnSet;
        }

        internal static object FindElement(Dictionary<object, object> source, Dictionary<object, object> searchPath)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( searchPath == null ) //{ throw new ArgumentException( "SearchPath cannot be null.", "searchPath" ); }
                return source;

            object result = null;

            foreach( object key in searchPath.Keys )
            {
                if( source.ContainsKey( key ) )
                {
                    if( source[key] is Dictionary<object, object> )
                        if( searchPath[key] is Dictionary<object, object> )
                            result = FindElement( (Dictionary<object, object>)source[key], (Dictionary<object, object>)searchPath[key] );
                        else
                            result = source[key];
                    else if( source[key] is List<object> )
                        result = FindElement( (List<object>)source[key], (List<object>)searchPath[key] );
                    else //( source[key] is string )
                        result = source[key];
                }
            }

            return result;
        }

        internal static object FindElement(List<object> source, List<object> searchPath)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( searchPath == null ) //{ throw new ArgumentException( "SearchPath cannot be null.", "searchPath" ); }
                return source;

            object result = null;

            Dictionary<object, object> patchItem = (Dictionary<object, object>)searchPath[0];

            //get the metadata index value, remove metadata
            int i = int.Parse( patchItem[__sli].ToString() );
            patchItem.Remove( __sli );

            if( patchItem.Count > 0 )
            {
                //get the value to updated/added at index 'i'
                object patchKey = patchItem.ElementAt( 0 ).Key;
                object patchValue = patchItem[patchKey];


                if( i >= 0 && i < source.Count )
                {
                    //updated syntax to Pattern Matching: if source[i] is Dict => listItemValue
                    if( source[i] is Dictionary<object, object> listItemValue )
                    {
                        if( patchValue is Dictionary<object, object> )
                            result = FindElement( (Dictionary<object, object>)listItemValue[patchKey], (Dictionary<object, object>)patchValue );
                        else if( patchValue is List<object> )
                            result = FindElement( (List<object>)listItemValue[patchKey], (List<object>)patchValue );
                        else //if( patchValue is 'the value' )
                            result = listItemValue[patchKey];
                    }
                    else if( source[i] is List<object> )
                        result = FindElement( (List<object>)source[i], (List<object>)patchKey );
                    else
                        result = source[i];
                }
                //else  //todo: return source?
                //    //i is outside source lbound/ubound
                //    result = source;
            }
            else
            {
                if( i >= 0 && i < source.Count )
                    result = source[i];
            }


            return result;
        }
        #endregion


        #region util
        /// <summary>
        /// Description: keeps looping until the bottom of the path, then appends the value (if value != null)
        /// Returns: a Yaml Dictionary<object, object>, where:
        /// Dict objects are:
        ///   Item:
        ///     Item:
        ///       Item: value
        /// Lists items are:
        ///   Item:
        ///   - SynapseListIndex: indexValue  - this represents metadata about where to put the value in the destination
        ///   where the following Item will be placed at [indexValue] in the destination list
        /// </summary>
        /// <param name="path">The path to convert to a YAML Dictionary<object, object></param>
        /// <param name="value">The terminus value of the path.</param>
        /// <returns>Path as a deserialized YAML Dictionary<object, object>.</returns>
        internal static Dictionary<object, object> ConvertPathElementToDict(string path, object value = null)
        {
            StringBuilder yaml = new StringBuilder();

            bool isList = false;
            string[] lines = path.Split( ':' );
            for( int i = 0; i < lines.Length; i++ )
            {
                int index = -1;
                isList = false;
                string newLine = CheckPathElementIsIndexed( lines[i], out index );
                yaml.AppendLine( newLine.PadLeft( (2 * i) + newLine.Length ) + ":" );

                //appends [- SynapseListIndex: indexValue]
                if( index > -1 )
                {
                    isList = true;
                    string sli = $"- {__sli}: {index}";
                    yaml.AppendLine( sli.PadLeft( (2 * i) + sli.Length ) );
                }
            }

            string buf = yaml.ToString().Trim();
            if( value != null )
                if( isList )
                {
                    string v = "Value";
                    yaml.AppendLine( $"{v.PadLeft( (2 * lines.Length) + v.Length )}: {__token}" );
                    buf = yaml.ToString().Trim();
                }
                else
                    buf = $"{buf} {__token}";

            Dictionary<object, object> yamldoc = Deserialize( buf );

            if( value != null )
                ReplaceToken( yamldoc, value );

            return yamldoc;
        }

        internal static bool ReplaceToken(Dictionary<object, object> source, object value)
        {
            bool found = false;

            foreach( object key in source.Keys )
            {
                if( source[key] is Dictionary<object, object> )
                    found = ReplaceToken( (Dictionary<object, object>)source[key], value );
                else if( source[key] is List<object> )
                    found = ReplaceToken( (List<object>)source[key], value );
                else if( source[key] is string && ((string)source[key]) == __token )
                {
                    source[key] = value;
                    found = true;
                }

                if( found )
                    break;
            }

            return found;
        }

        internal static bool ReplaceToken(List<object> source, object value)
        {
            bool found = false;

            for( int i = 0; i < source.Count; i++ )
            {
                if( source[i] is Dictionary<object, object> )
                    found = ReplaceToken( (Dictionary<object, object>)source[i], value );
                else if( source[i] is List<object> )
                    found = ReplaceToken( (List<object>)source[i], value );
                else if( source[i] is string && ((string)source[i]) == __token )
                {
                    source[i] = value;
                    found = true;
                }

                if( found )
                    break;
            }

            return found;
        }


        //internal static Dictionary<object, object> ConvertPathElementToDict(string path, string value = null)
        //{
        //    StringBuilder yaml = new StringBuilder();

        //    string[] lines = path.Split( ':' );
        //    for( int i = 0; i < lines.Length; i++ )
        //    {
        //        int index = -1;
        //        string newLine = CheckPathElementIsIndexed( lines[i], out index );
        //        yaml.AppendLine( newLine.PadLeft( (2 * i) + newLine.Length ) + ":" );

        //        if( index > -1 )
        //        {
        //            string sli = $"- {__sli}: {index}";
        //            yaml.AppendLine( sli.PadLeft( (2 * i) + sli.Length ) );
        //        }
        //    }

        //    string buf = yaml.ToString().Trim();
        //    if( !string.IsNullOrWhiteSpace( value ) )
        //    {
        //        if( value.Contains( ":" ) ) value = EncapsulateWith( value, "'" );
        //        buf = $"{buf} {value}";
        //    }

        //    return Deserialize( buf );
        //}

        internal static string CheckPathElementIsIndexed(string element, out int index)
        {
            index = -1;
            if( element.EndsWith( "]" ) )
            {
                int left = element.LastIndexOf( '[' );
                string num = element.Substring( left, element.Length - left );
                index = int.Parse( Regex.Match( num, @"\d+" ).Value );
                element = element.Substring( 0, left );
            }

            return element;
        }


        //internal static string EncapsulateWith(string s, string c)
        //{
        //    if( !s.StartsWith( c ) ) { s = $"{c}{s}"; }
        //    if( !s.EndsWith( c ) ) { s = $"{s}{c}"; }
        //    return s;
        //}
        #endregion
    }
}