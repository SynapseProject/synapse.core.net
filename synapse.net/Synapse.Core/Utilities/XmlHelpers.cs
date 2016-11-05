using System;
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
    }
}