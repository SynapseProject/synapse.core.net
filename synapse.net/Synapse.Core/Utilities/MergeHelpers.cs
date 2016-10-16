using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.XmlDiffPatch;
using YamlDotNet.Serialization;

namespace Synapse.Core.Utilities
{
    public class MergeHelpers
    {
        #region xml
        //works, but is horrifically inefficient.
        //todo: rewrite to calc all xpaths upfront, then select/update from source
        public static void MergeXml(ref XmlDocument source, XmlDocument patch)
        {
            Stack<IEnumerable> lists = new Stack<IEnumerable>();
            lists.Push( patch.ChildNodes );

            while( lists.Count > 0 )
            {
                IEnumerable list = lists.Pop();
                foreach( XmlNode node in list )
                {
                    string xpath = FindXPath( node );
                    XmlNode src = source.SelectSingleNode( xpath );
                    if( src != null && src.Value != node.Value )
                    {
                        if( src.NodeType == XmlNodeType.Element )
                        {
                            src.InnerText = node.InnerText;
                        }
                        else
                        {
                            src.Value = node.Value;
                        }
                    }
                    if( node.Attributes != null )
                    {
                        lists.Push( node.Attributes );
                    }
                    if( node.ChildNodes.Count > 0 )
                    {
                        lists.Push( node.ChildNodes );
                    }
                }
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
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException( "Couldn't find element within parent" );
        }

        public static void MergeXml(ref XmlDocument source, List<DynamicValue> patch, Dictionary<string, string> values)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }
            if( values == null ) { throw new ArgumentException( "Values cannot be null.", "values" ); }

            foreach( DynamicValue v in patch )
            {
                if( values.ContainsKey( v.Name ) )
                {
                    XmlNode src = source.SelectSingleNode( v.Path );
                    if( src != null )
                    {
                        if( src.NodeType == XmlNodeType.Element )
                        {
                            src.InnerText = values[v.Name];
                        }
                        else
                        {
                            src.Value = values[v.Name];
                        }
                    }
                }
            }
        }
        #endregion

        #region yaml/json
        public static Dictionary<object, object> DeserializeYaml(string source)
        {
            Dictionary<object, object> result = new Dictionary<object, object>();
            using( StringReader sr = new StringReader( source ) )
            {
                Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
                result = deserializer.Deserialize( sr ) as Dictionary<object, object>;
            }
            return result;
        }

        public static void MergeYaml(ref Dictionary<object, object> source, Dictionary<object, object> patch)
        {
            ApplyPatchValuesYaml( source, patch );
        }

        public static void MergeYaml(ref Dictionary<object, object> source, List<DynamicValue> patch, Dictionary<string, string> values)
        {
            Dictionary<object, object> p = ConvertDynamicValuestoDict( patch, values );
            ApplyPatchValuesYaml( source, p );
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

        static void ApplyPatchValuesYaml(Dictionary<object, object> source, Dictionary<object, object> patch)
        {
            if( source == null ) { throw new ArgumentException( "Source cannot be null.", "source" ); }
            if( patch == null ) { throw new ArgumentException( "Patch cannot be null.", "patch" ); }

            foreach( object key in patch.Keys )
            {
                if( source.ContainsKey( key ) && patch[key] is Dictionary<object, object> )
                    ApplyPatchValuesYaml( (Dictionary<object, object>)source[key], (Dictionary<object, object>)patch[key] );
                else
                    source[key] = patch[key];
            }
        }
        #endregion


        #region doesn't work as-is, need to try file-->fragments
        //reference: https://msdn.microsoft.com/en-us/library/aa302295.aspx
        //source:	 https://msdn.microsoft.com/en-us/library/aa302294.aspx
        public static void MergeXml(ref XmlNode sourceNode, XmlNode changedNode)
        {
            XmlDiff xmldiff = new XmlDiff( XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl |
                XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePrefixes );

            MemoryStream ms = new MemoryStream();
            XmlTextWriter diffgramWriter = new XmlTextWriter( ms, Encoding.Default );
            bool match = xmldiff.Compare( sourceNode, changedNode, diffgramWriter );
            ms.Position = 0;

            using( XmlTextReader diffgramReader = new XmlTextReader( ms ) )
            {
                XmlPatch xmlpatch = new XmlPatch();
                xmlpatch.Patch( ref sourceNode, diffgramReader );
            }

            diffgramWriter.Close();
        }

        public void GenerateDiffGram(string originalFile, string finalFile, XmlWriter diffgramWriter)
        {
            XmlDiff xmldiff = new XmlDiff( XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl |
                XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePrefixes );

            bool bIdentical = xmldiff.Compare( originalFile, finalFile, false, diffgramWriter );
            diffgramWriter.Close();
        }

        public void PatchUp(string originalFile, String diffgramFile, String outputFile)
        {
            XmlDocument sourceDoc = new XmlDocument( new NameTable() );
            sourceDoc.Load( originalFile );

            using( XmlTextReader diffgramReader = new XmlTextReader( diffgramFile ) )
            {
                XmlPatch xmlpatch = new XmlPatch();
                xmlpatch.Patch( sourceDoc, diffgramReader );

                using( XmlTextWriter output = new XmlTextWriter( outputFile, Encoding.Unicode ) )
                {
                    sourceDoc.Save( output );
                }
            }
        }
        #endregion
    }
}