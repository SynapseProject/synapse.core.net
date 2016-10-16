using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Synapse.Core.Utilities;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public partial class ParameterInfo : IParameterInfo
    {
        private Dictionary<string, string> _dynamicData = null;

        public string Resolve(Dictionary<string, string> dynamicData = null)
        {
            _dynamicData = dynamicData ?? new Dictionary<string, string>();

            string parms = string.Empty;
            switch( Type )
            {
                case SerializationType.Xml:
                {
                    parms = ResolveXml();
                    break;
                }
                case SerializationType.Json:
                {
                    parms = ResolveJson();
                    break;
                }
                case SerializationType.Yaml:
                {
                    parms = ResolveYaml();
                    break;
                }
                case SerializationType.Unspecified:
                {
                    parms = ResolveUnspecified();
                    break;
                }
            }

            ResolvedValuesSerialized = parms;
            return parms;
        }

        string ResolveXml()
        {
            XmlDocument parms = null;

            if( HasInheritedValues )
            {
                parms = (XmlDocument)InheritedValues;
            }

            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );
                XmlDocument uriXml = new XmlDocument();
                uriXml.LoadXml( uriContent );

                if( parms != null )
                {
                    Utilities.MergeHelpers.MergeXml( ref parms, uriXml );
                }
                else
                {
                    parms = uriXml;
                }
            }

            //merge parms
            if( HasValues )
            {
                XmlDocument values = new XmlDocument();
                values.LoadXml( Values.ToString() );

                if( parms != null )
                {
                    Utilities.MergeHelpers.MergeXml( ref parms, values );
                }
                else
                {
                    parms = values;
                }
            }

            //kv_replace
            if( HasDynamic && parms != null )
            {
                Utilities.MergeHelpers.MergeXml( ref parms, Dynamic, _dynamicData );
            }

            //todo: XmlSerializer
            return parms.ToString();
        }

        string ResolveJson()
        {
            object parms = null;

            if( HasInheritedValues )
            {
                parms = InheritedValues;
            }

            //make rest call
            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );

                if( parms != null )
                {
                    object values = null;
                    using( StringReader sr = new StringReader( uriContent ) )
                    {
                        Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
                        values = deserializer.Deserialize( sr );
                    }

                    Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                    Dictionary<object, object> uv = (Dictionary<object, object>)values;
                    Utilities.MergeHelpers.MergeYaml( ref ip, uv );
                }
                else
                {
                    using( StringReader sr = new StringReader( uriContent ) )
                    {
                        Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
                        parms = deserializer.Deserialize( sr );
                    }
                }
            }

            Dictionary<object, object> p = (Dictionary<object, object>)parms;

            //merge parms
            if( HasValues && p != null )
            {
                Utilities.MergeHelpers.MergeYaml( ref p, (Dictionary<object, object>)Values );
            }

            //kv_replace
            if( HasDynamic && p != null )
            {
                Utilities.MergeHelpers.MergeYaml( ref p, Dynamic, _dynamicData );
            }

            string v = null;
            using( StringWriter sw = new StringWriter() )
            {
                Serializer serializer = new Serializer();
                serializer.Serialize( sw, parms );
                v = sw.ToString();
            }

            return v;
        }

        string ResolveYaml()
        {
            object parms = null;

            //make rest call
            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );

                using( StringReader sr = new StringReader( uriContent ) )
                {
                    Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
                    parms = deserializer.Deserialize( sr );
                }
            }

            if( parms == null )
                parms = new Dictionary<object, object>();
            Dictionary<object, object> p = (Dictionary<object, object>)parms;

            //merge parms
            if( HasValues )
            {
                if( p == null )
                    p = new Dictionary<object, object>();
                Utilities.MergeHelpers.MergeYaml( ref p, (Dictionary<object, object>)Values );
            }

            //kv_replace
            if( HasDynamic )
            {
                if( p == null )
                    p = new Dictionary<object, object>();
                Utilities.MergeHelpers.MergeYaml( ref p, Dynamic, _dynamicData );
            }

            string v = null;
            using( StringWriter sw = new StringWriter() )
            {
                Serializer serializer = new Serializer();
                serializer.Serialize( sw, parms );
                v = sw.ToString();
            }

            return v;
        }

        string ResolveUnspecified()
        {
            string parms = string.Empty;

            //make rest call
            if( HasUri )
            {
                parms = string.Empty;
            }

            //merge parms
            if( HasValues )
            {
            }

            if( HasDynamic )
            {
                //kv_replace
            }

            return parms;
        }

        async Task<string> GetUri(string uri)
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync( uri );
        }
    }
}