using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Synapse.Core.Utilities
{
    public class XmlHelpers
    {
        public static string Serialize<T>(object data, bool omitXmlDeclaration = true, bool omitXmlNamespace = true,
            bool indented = false, Encoding encoding = null)
        {
            if( encoding == null )
                encoding = UnicodeEncoding.Unicode;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = omitXmlDeclaration;
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseOutput = false;
            settings.Encoding = encoding;
            settings.Indent = indented;

            MemoryStream ms = new MemoryStream();
            XmlSerializer s = new XmlSerializer( typeof( T ) );
            XmlWriter w = XmlWriter.Create( ms, settings );
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
            string result = encoding.GetString( ms.GetBuffer(), 0, (int)ms.Length );
            w.Close();
            return result;
        }

        public static T Deserialize<T>(TextReader reader)
        {
            XmlSerializer s = new XmlSerializer( typeof( T ) );
            return (T)s.Deserialize( reader );
        }

        #region merge
        //works, but is horrifically inefficient.
        //todo: rewrite to calc all xpaths upfront, then select/update from source
        public static void Merge(ref XmlDocument source, XmlDocument patch)
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
                        if( src.NodeType == XmlNodeType.Element )
                            src.InnerText = node.InnerText;
                        else
                            src.Value = node.Value;

                    if( node.Attributes != null )
                        lists.Push( node.Attributes );

                    if( node.ChildNodes.Count > 0 )
                        lists.Push( node.ChildNodes );
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
                        return index;

                    index++;
                }
            }
            throw new ArgumentException( "Couldn't find element within parent" );
        }

        public static void Merge(ref XmlDocument source, List<DynamicValue> patch, Dictionary<string, string> values)
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
                        if( src.NodeType == XmlNodeType.Element )
                            src.InnerText = values[v.Name];
                        else
                            src.Value = values[v.Name];
                }
            }
        }
        #endregion

        #region ForEach
        internal static List<object> ExpandForEachAndApplyPatchValues(ref XmlDocument source, List<ForEach> forEach)
        {
            ForEach node = forEach[0];
            for( int i = 1; i < forEach.Count; i++ )
            {
                node.Child = forEach[i];
                node = forEach[i];
            }

            List<object> matrix = new List<object>();
            ExpandMatrixApplyPatchValues( forEach[0], source, matrix );

            return matrix;
        }

        static void ExpandMatrixApplyPatchValues(ForEach fe, XmlDocument source, List<object> matrix)
        {
            foreach( string v in fe.Values )
            {
                XmlNode src = source.SelectSingleNode( fe.Path );
                if( src != null )
                    if( src.NodeType == XmlNodeType.Element )
                        src.InnerText = v;
                    else
                        src.Value = v;

                if( fe.HasChild )
                    ExpandMatrixApplyPatchValues( fe.Child, source, matrix );
                else
                    matrix.Add( source );
            }
        }
        #endregion
    }
}