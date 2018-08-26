using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

using Synapse.Core.Utilities;

namespace Synapse.Core
{
    [DataContract()]
    public class StartPlanEnvelope
    {
        public StartPlanEnvelope()
        {
        }


        [XmlIgnore]
        public Plan Plan { get; set; }
        [XmlIgnore]
        public Dictionary<string, string> DynamicParameters { get; set; } = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        public Dictionary<string, string> GetCaseInsensitiveDynamicParameters() { return new Dictionary<string, string>( DynamicParameters, StringComparer.OrdinalIgnoreCase ); }

        public Dictionary<string, string> GetCaseInsensitiveDynamicParametersFromXml()
        {
            Dictionary<string, string> parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
            foreach( Param p in XmlDynamicParameters.Parm )
                parms[p.Name] = p.Text;

            XmlDynamicParameters = null;

            return parms;
        }

        public Dictionary<string, string> TryGetCaseInsensitiveDynamicParameters()
        {
            if( DynamicParameters.Count > 0 )
                return GetCaseInsensitiveDynamicParameters();
            else if( XmlDynamicParameters.Parm.Count > 0 )
                return GetCaseInsensitiveDynamicParametersFromXml();
            else
                return null;
        }

        [XmlElement]
        [DataMember]
        public XmlDynamicParameters XmlDynamicParameters { get; set; } = new XmlDynamicParameters();


        public string ToYaml(bool encode = false)
        {
            string yaml = null;

            using( StringWriter s = new StringWriter() )
                yaml = YamlHelpers.Serialize( this );

            if( encode )
                yaml = CryptoHelpers.Encode( yaml );

            return yaml;
        }

        public static StartPlanEnvelope FromYaml(string yaml, bool isEncoded = false)
        {
            if( isEncoded )
                yaml = CryptoHelpers.Decode( yaml );

            return YamlHelpers.Deserialize<StartPlanEnvelope>( yaml, ignoreUnmatchedProperties: false );
        }

        public string ToXml(bool encode = false)
        {
            string xml = null;

            xml = XmlHelpers.Serialize<StartPlanEnvelope>( this );

            if( encode )
                xml = CryptoHelpers.Encode( xml );

            return xml;
        }

        public static StartPlanEnvelope FromXml(string xml, bool isEncoded = false)
        {
            if( isEncoded )
                xml = CryptoHelpers.Decode( xml );

            return XmlHelpers.Deserialize<StartPlanEnvelope>( xml );
        }
    }

    [DataContract()]
    public class Param
    {
        [XmlAttribute]
        [DataMember]
        public string Name { get; set; }
        [XmlText]
        [DataMember]
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Name}::{Text}";
        }
    }

    [DataContract()]
    public class XmlDynamicParameters
    {
        [XmlElement]
        [DataMember]
        public List<Param> Parm { get; set; } = new List<Param>();
    }
}