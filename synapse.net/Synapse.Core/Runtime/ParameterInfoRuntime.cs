using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Synapse.Core.Utilities;
using YamlDotNet.Serialization;
using System.Text;

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
                case SerializationType.Yaml:
                case SerializationType.Json:
                {
                    parms = ResolveYamlJson();
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
                    MergeHelpers.MergeXml( ref parms, uriXml );
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
                    MergeHelpers.MergeXml( ref parms, values );
                }
                else
                {
                    parms = values;
                }
            }

            //kv_replace
            if( HasDynamic && parms != null )
            {
                MergeHelpers.MergeXml( ref parms, Dynamic, _dynamicData );
            }

            return parms.OuterXml;
        }

        string ResolveYamlJson()
        {
            object parms = null;

            if( HasInheritedValues )
                parms = InheritedValues;

            //make rest call
            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );

                if( parms != null )
                {
                    object values = null;
                    using( StringReader sr = new StringReader( uriContent ) )
                    {
                        values = YamlHelpers.Deserialize<object>( sr );
                    }

                    Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                    Dictionary<object, object> uv = (Dictionary<object, object>)values;
                    MergeHelpers.MergeYaml( ref ip, uv );
                }
                else
                {
                    using( StringReader sr = new StringReader( uriContent ) )
                    {
                        parms = YamlHelpers.Deserialize<object>( sr );
                    }
                }
            }

            if( parms == null )
                parms = new Dictionary<object, object>();
            Dictionary<object, object> p = (Dictionary<object, object>)parms;

            //merge parms
            if( HasValues && p != null )
                MergeHelpers.MergeYaml( ref p, (Dictionary<object, object>)Values );

            //kv_replace
            if( HasDynamic && p != null )
                MergeHelpers.MergeYaml( ref p, Dynamic, _dynamicData );

            return YamlHelpers.Serialize( parms );
        }

        string ResolveUnspecified()
        {
            string parms = null;

            if( HasInheritedValues )
                parms = InheritedValues.ToString();

            //make rest call
            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );
                parms += uriContent;
            }

            //merge parms
            if( HasValues )
            {
                parms += Values.ToString();
            }

            if( HasDynamic )
            {
                StringBuilder sb = new StringBuilder();
                foreach(string key in _dynamicData.Keys)
                {
                    sb.Append( $"{key}:{_dynamicData[key]}," );
                }
                parms += sb.ToString().TrimEnd( ',' );
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