using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Synapse.Core.Utilities
{
    public class XmlHelpers
    {
        public static string Serialize<T>(object data, bool omitXmlDeclaration = true, bool omitXmlNamespace = true,
            bool indented = true, Encoding encoding = null)
        {
            if( string.IsNullOrWhiteSpace( data?.ToString() ) )
                return null;

            if( encoding == null )
                encoding = UnicodeEncoding.UTF8;

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = omitXmlDeclaration,
                ConformanceLevel = ConformanceLevel.Auto,
                Encoding = encoding,
                CloseOutput = false,
                Indent = indented
            };

            XmlSerializer s = new XmlSerializer( typeof( T ) );
            StringBuilder buf = new StringBuilder();
            XmlWriter w = XmlWriter.Create( buf, settings );
            if( data is XmlDocument )
            {
                ((XmlDocument)data).Save( w );
            }
            else
            {
                if( omitXmlNamespace )
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add( "", "" );
                    s.Serialize( w, data, ns );
                }
                else
                {
                    s.Serialize( w, data );
                }
            }
            w.Close();

            return buf.ToString();
        }

        public static T Deserialize<T>(string s)
        {
            StringReader sr = new StringReader( s );
            return Deserialize<T>( sr );
        }

        public static T Deserialize<T>(TextReader reader)
        {
            XmlSerializer s = new XmlSerializer( typeof( T ) );
            return (T)s.Deserialize( reader );
        }


        #region merge
        //works, but is horrifically inefficient.
        //todo: rewrite to calc all xpaths upfront, then select/update from source
        public static void Merge(ref XmlDocument source, XmlDocument patch, bool hackSkipImport = false)
        {
            if( source.DocumentElement == null )
                source.LoadXml( patch.OuterXml );
            else
            {
                if( !hackSkipImport )
                    foreach( XmlNode node in patch.DocumentElement.ChildNodes )
                    {
                        string xpath = FindXPath( node );
                        XmlNode src = source.SelectSingleNode( xpath );
                        if( src == null )
                        {
                            XmlNode imported = source.ImportNode( node, true );
                            source.DocumentElement.AppendChild( imported );
                        }
                    }

                //loop through all the nodes/attributes and sync the values
                Stack<IEnumerable> lists = new Stack<IEnumerable>();
                lists.Push( patch.ChildNodes );

                while( lists.Count > 0 )
                {
                    IEnumerable list = lists.Pop();
                    foreach( XmlNode node in list )
                    {
                        string xpath = FindXPath( node );
                        XmlNode src = source.SelectSingleNode( xpath );

                        if( src != null )
                        {
                            if( src.Value != node.Value )
                                if( src.NodeType == XmlNodeType.Element )
                                    if( src.SelectNodes( "*" ).Count > 0 )
                                        src.InnerXml = node.InnerXml;
                                    else
                                        src.InnerText = node.InnerText;
                                else
                                    src.Value = node.Value;
                        }
                        else
                            AddNodeToDocument( ref source, xpath, node );

                        if( node.Attributes != null && node.Attributes.Count > 0 )
                            lists.Push( node.Attributes );

                        if( node.ChildNodes.Count > 0 )
                            lists.Push( node.ChildNodes );
                    }
                }
            }
        }

        static void AddNodeToDocument(ref XmlDocument source, string xpath, XmlNode node)
        {
            xpath = xpath.Substring( 0, xpath.LastIndexOf( '/' ) );
            if( !string.IsNullOrWhiteSpace( xpath ) )
            {
                XmlNode src = source.SelectSingleNode( xpath );
                if( src != null )
                    src.AppendChild( source.ImportNode( node, true ) );
            }
        }

        static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while( node != null )
            {
                switch( node.NodeType )
                {
                    case XmlNodeType.Attribute:
                    {
                        builder.Insert( 0, "/@" + node.Name );
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    }
                    case XmlNodeType.Element:
                    {
                        int index = FindElementIndex( (XmlElement)node );
                        builder.Insert( 0, "/" + node.Name + "[" + index + "]" );
                        node = node.ParentNode;
                        break;
                    }
                    case XmlNodeType.Document:
                    {
                        return builder.ToString();
                    }
                    case XmlNodeType.Text:
                    {
                        node = node.ParentNode;
                        break;
                    }
                    //default:
                    //{
                    //	throw new ArgumentException( "Only elements and attributes are supported" );
                    //}
                }
            }
            throw new ArgumentException( "Node was not in a document" );
        }

        static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if( parentNode is XmlDocument )
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach( XmlNode candidate in parent.ChildNodes )
            {
                if( candidate is XmlElement && candidate.Name == element.Name )
                {
                    if( candidate == element )
                        return index;

                    index++;
                }
            }
            throw new ArgumentException( "Couldn't find element within parent" );
        }

        public static void Merge(ref XmlDocument source, List<DynamicValue> dynamicValues, Dictionary<string, string> values)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }

            //if there's nothing to do, get out!
            if( dynamicValues == null || dynamicValues?.Count == 0 )
                return;

            foreach( DynamicValue dv in dynamicValues )
            {
                string value = null;
                if( (values?.ContainsKey( dv.Source )).Value && !string.IsNullOrWhiteSpace( values[dv.Source] ) )
                    value = values[dv.Source];

                if( string.IsNullOrWhiteSpace( value ) && dv.TryGetDefaultValue( out object defaultValue ) )
                    if( defaultValue != null )
                        value = defaultValue.ToString();

                if( !string.IsNullOrWhiteSpace( value ) || dv.DefaultAllowNull )
                {
                    if( !dv.Validate( value, out string validationErrorMessage ) )
                        throw new ArgumentException( validationErrorMessage );

                    XmlNode src = source.SelectSingleNode( dv.Target );
                    if( src != null )
                        if( src.NodeType == XmlNodeType.Element )
                            if( src.SelectNodes( "*" ).Count > 0 )
                                src.InnerXml = RegexReplaceOrValue( src.InnerXml, value, dv );
                            else
                                src.InnerText = RegexReplaceOrValue( src.InnerText, value, dv );
                        else
                            src.Value = RegexReplaceOrValue( src.Value, value, dv );
                    else
                    {
                        XmlDocument doc = XPathToXmlDocument( dv.Target, value );

                        if( source.DocumentElement == null )
                            source.LoadXml( doc.OuterXml );
                        else
                            Merge( ref source, doc );
                    }
                }
            }
        }

        public static void Merge(ref XmlDocument destination, List<ParentExitDataValue> parentExitData, ref XmlDocument values)
        {
            IEnumerable<ParentExitDataValue> transforms = parentExitData.Where( pex => pex.HasTransformInPlace );
            foreach( ParentExitDataValue ped in transforms )
            {
                XmlDocument xf = TransformXml( values, ped.TransformInPlace.Source );
                XmlDocument patch = XPathToXmlDocument( ped.TransformInPlace.Target, null );
                Merge( ref patch, xf, hackSkipImport: true );
                Merge( ref values, patch );
            }

            foreach( ParentExitDataValue ped in parentExitData )
            {
                string value = null;
                XmlNode src = values.SelectSingleNode( ped.Source );
                if( src != null )
                {
                    //if( ped.CastToForEachItems )
                    //{
                    //    if( forEach == null )
                    //        forEach = new List<ForEachItem>();

                    //    ForEachItem fe = new ForEachItem() { Target = ped.CopyToValues.Target };
                    //    forEach.Add( fe );

                    //    if( src.NodeType == XmlNodeType.Element )
                    //    {
                    //        XmlNodeList nodes = src.SelectNodes( "*" );
                    //        if( nodes.Count > 0 )
                    //            foreach( XmlNode node in nodes )
                    //                fe.Values.Add( node.InnerXml ); //InnerText or just the node?
                    //        else
                    //            fe.Values.Add( src.InnerText );
                    //    }
                    //    else
                    //        fe.Values.Add( src.Value );
                    //}
                    //else
                    //{
                        if( src.NodeType == XmlNodeType.Element )
                            if( src.SelectNodes( "*" ).Count > 0 )
                                value = src.InnerXml;
                            else
                                value = src.InnerText;
                        else
                            value = src.Value;
                        XmlNode dst = destination.SelectSingleNode( ped.CopyToValues.Target );
                        if( dst != null )
                            if( dst.NodeType == XmlNodeType.Element )
                                if( src.SelectNodes( "*" ).Count > 0 )
                                    dst.InnerXml = RegexReplaceOrValue( dst.InnerXml, value, ped.CopyToValues );
                                else
                                    dst.InnerText = RegexReplaceOrValue( dst.InnerText, value, ped.CopyToValues );
                            else
                                dst.Value = RegexReplaceOrValue( dst.Value, value, ped.CopyToValues );
                        else
                        {
                            XmlDocument patch = XPathToXmlDocument( ped.CopyToValues.Target, value );

                            if( destination.DocumentElement == null )
                                destination.LoadXml( patch.OuterXml );
                            else
                                Merge( ref destination, patch );
                        }
                    //}
                }
            }
        }

        internal static void SelectForEachFromValues(List<ForEachItem> parameterSetSources, ref XmlDocument values,
            ConcurrentDictionary<string, ParameterInfo> globalParamSets, object parentExitData)
        {
            foreach( ForEachItem fe in parameterSetSources )
            {
                ParameterSourceInfo pss = fe.ParameterSource;

                XmlDocument v = pss.HasName ? GetParamSet( pss.Name, globalParamSets, pss.IsNameParentExitData, parentExitData ) : values;
                XmlNode src = v?.SelectSingleNode( pss.Source );
                if( src != null )
                {
                    if( src.NodeType == XmlNodeType.Element )
                    {
                        XmlNodeList nodes = src.SelectNodes( "*" );
                        if( nodes.Count > 0 )
                            foreach( XmlNode node in nodes )
                                fe.Values.Add( node.InnerXml ); //InnerText or just the node?
                        else
                            fe.Values.Add( src.InnerText );
                    }
                    else
                        fe.Values.Add( src.Value );
                }
            }
        }

        static XmlDocument GetParamSet(string key, ConcurrentDictionary<string, ParameterInfo> globalParamSets, bool isNameParentExitData, object parentExitData)
        {
            XmlDocument p = null;

            if( isNameParentExitData )
                p = parentExitData as XmlDocument;
            else if( globalParamSets.ContainsKey( key ) )
                p = globalParamSets[key].Values as XmlDocument;

            return p;
        }

        internal static object TryParseValue(object value)
        {
            if( value is string )
                try
                {
                    string s = value as string;
                    if( !string.IsNullOrWhiteSpace( s ) )
                        value = Deserialize<object>( s );
                }
                catch( Exception ex ) { string e = ex.Message; }//eat the error

            return value;
        }

        internal static string RegexReplaceOrValue(string input, string replacement, IReplacementValueOptions rv)
        {
            string value = replacement;

            if( rv != null )
            {
                if( rv.HasReplace )
                {
                    if( rv.IsBase64Encode )
                        replacement = CryptoHelpers.Encode( replacement );
                    value = Regex.Replace( input, rv.Replace, replacement, RegexOptions.IgnoreCase );
                }
                else if( rv.IsBase64Encode )
                    value = CryptoHelpers.Encode( value );
            }

            return value;
        }

        //todo: this feels insufficient: it only handles simple element/attribute syntax
        static XmlDocument XPathToXmlDocument(string xpath, string value)
        {
            StringBuilder xml = new StringBuilder();

            //remove indexes, get path parts
            xpath = Regex.Replace( xpath, @"\[\d\]", string.Empty );
            string[] branches = xpath.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );

            //create the opening tags
            for( int i = 0; i < branches.Length; i++ )
            {
                string cur = branches[i];
                bool nextIsAttr = i + 1 < branches.Length && branches[i + 1].StartsWith( "@" );
                string tag = nextIsAttr ? " " : ">";
                xml.Append( cur.StartsWith( "@" ) ? $" {cur.Replace( "@", string.Empty )}=\"{value}\">" : $"<{cur}{tag}" );
            }

            //include the 'value'
            if( !branches[branches.Length - 1].StartsWith( "@" ) )
                xml.Append( value );

            //create the closing tags
            for( int i = branches.Length - 1; i >= 0; i-- )
                xml.Append( branches[i].StartsWith( "@" ) ? string.Empty : $"</{branches[i]}>" );

            //create the return doc
            XmlDocument merge = new XmlDocument();
            merge.LoadXml( xml.ToString() );

            return merge;
        }

        internal static XmlDocument TransformXml(XmlDocument xml, string xslt)
        {
            if( string.IsNullOrWhiteSpace( xslt ) || xml == null )
                return xml;

            // Process the XML
            XPathDocument xpathDocument = new XPathDocument( new XmlNodeReader( xml ) );

            // Process the XSLT
            XmlTextReader xmlTextReaderXslt = new XmlTextReader( new StringReader( xslt ) );
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load( xmlTextReaderXslt );

            // Handle the output stream
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Auto,
                Encoding = Encoding.UTF8,
                CloseOutput = true,
                Indent = true
            };

            StringBuilder buf = new StringBuilder();
            XmlWriter writer = XmlWriter.Create( buf, settings );

            // Do the transform
            xslCompiledTransform.Transform( xpathDocument, null, writer );

            // Return unformatted string
            XmlDocument doc = new XmlDocument();
            doc.LoadXml( buf.ToString() );

            return doc;
        }
        #endregion


        #region ForEach
        internal static List<object> ExpandForEachAndApplyPatchValues(ref XmlDocument source, List<ForEachItem> forEach)
        {
            ForEachItem node = forEach[0];
            for( int i = 1; i < forEach.Count; i++ )
            {
                node.Child = forEach[i];
                node = forEach[i];
            }

            List<object> matrix = new List<object>();
            ExpandMatrixApplyPatchValues( forEach[0], source, matrix );

            return matrix;
        }

        static void ExpandMatrixApplyPatchValues(ForEachItem fe, XmlDocument source, List<object> matrix)
        {
            foreach( string value in fe.Values )
            {
                XmlNode src = source.SelectSingleNode( fe.Target );
                if( src != null )
                    if( src.NodeType == XmlNodeType.Element )
                        if( src.SelectNodes( "*" ).Count > 0 )
                            src.InnerXml = RegexReplaceOrValue( src.InnerXml, value, fe );
                        else
                            src.InnerText = RegexReplaceOrValue( src.InnerText, value, fe );
                    else
                        src.Value = RegexReplaceOrValue( src.Value, value, fe );

                if( fe.HasChild )
                    ExpandMatrixApplyPatchValues( fe.Child, source, matrix );
                else
                    matrix.Add( (XmlDocument)source.Clone() );
            }
        }
        #endregion


        #region crypto
        public static T GetCryptoValues<T>(T c, ref XmlDocument source, bool isEncryptMode = true) where T : class, ICrypto
        {
            T result = null;

            if( c.HasCrypto )
            {
                c.Crypto.CreateInstance();
                c.Crypto.IsEncryptMode = isEncryptMode;

                List<string> errors = new List<string>();

                foreach( string element in c.Crypto.Elements )
                {
                    try
                    {
                        XmlNode src = source.SelectSingleNode( element );
                        if( src != null )
                        {
                            if( src.NodeType == XmlNodeType.Element )
                                if( src.SelectNodes( "*" ).Count > 0 )
                                    src.InnerXml = c.Crypto.SafeHandleCrypto( src.InnerXml );
                                else
                                    src.InnerText = c.Crypto.SafeHandleCrypto( src.InnerText );
                            else
                                src.Value = c.Crypto.SafeHandleCrypto( src.Value );
                        }
                        else
                            errors.Add( $"Element: [{element}] failed with error: [SelectSingleNode returned null]." );
                    }
                    catch( Exception ex )
                    {
                        errors.Add( $"Element: [{element}] failed with error: [{ex.Message.Trim()}]." );
                    }
                }

                string p = YamlHelpers.Serialize( c );
                result = YamlHelpers.Deserialize<T>( p );

                if( errors.Count == 0 )
                    result.Crypto.Errors = null;
                else
                    foreach( string error in errors )
                        result.Crypto.Errors.Add( error );

                c.Crypto.Errors = result.Crypto.Errors;
            }

            return result;
        }
        #endregion
    }
}

/*
 * Just for documentation, because I'll otherwise forget and try this again in the future:
 * 
 * The behavior of XmlDiffPatch is remove anything in 'source' that's not represented
 * in 'patch', which is not the desired behavior for Synapse.Merge functions. The
 * code below otherwise works.
 * 
 * The first foreach loop is to add any top level nodes from source not represented
 * in patch, then the following code executes a standard Diff/Patch.
 * 

    using Microsoft.XmlDiffPatch;
 
            foreach( XmlNode node in source.DocumentElement.ChildNodes )
            {
                string xpath = FindXPath( node );
                XmlNode p = patch.SelectSingleNode( xpath );
                if( p == null )
                {
                    XmlNode imported = patch.ImportNode( node, true );
                    patch.DocumentElement.AppendChild( imported );
                }
            }

            XmlReader sr = XmlReader.Create( new StringReader( source.OuterXml ) );
            XmlReader pr = XmlReader.Create( new StringReader( patch.OuterXml ) );
            StringBuilder dgw = new StringBuilder();
            XmlWriter dw = XmlWriter.Create( dgw );
            XmlDiff xmlDiff = new XmlDiff( XmlDiffOptions.IgnoreChildOrder );
            bool ok = xmlDiff.Compare( sr, pr, dw );
            XmlPatch xmlPatch = new XmlPatch();
            StringReader reader = new StringReader( dgw.ToString() );
            xmlPatch.Patch( source, XmlReader.Create( reader ) );

 */
