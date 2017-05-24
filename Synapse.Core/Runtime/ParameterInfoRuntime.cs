using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public partial class ParameterInfo : IParameterInfo
    {
        private Dictionary<string, string> _dynamicData = null;

        public object Resolve(out List<object> forEachParms, Dictionary<string, string> dynamicData = null)
        {
            _dynamicData = dynamicData ?? new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
            forEachParms = new List<object>();

            object parms = null;
            switch( Type )
            {
                case SerializationType.Xml:
                {
                    parms = ResolveXml( ref forEachParms );
                    break;
                }
                case SerializationType.Yaml:
                case SerializationType.Json:
                {
                    parms = ResolveYamlJson( ref forEachParms );
                    break;
                }
                case SerializationType.Unspecified:
                {
                    parms = ResolveUnspecified();
                    break;
                }
            }

            if( forEachParms.Count == 0 )
                forEachParms.Add( parms );

            Values = parms;

            return parms;
        }

        XmlDocument ResolveXml(ref List<object> forEachParms)
        {
            XmlDocument parms = null;

            if( HasInheritedValues )
                parms = (XmlDocument)((XmlDocument)((ParameterInfo)InheritedValues).Values).Clone();

            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );
                XmlDocument uriXml = new XmlDocument();
                uriXml.LoadXml( uriContent );

                if( parms != null )
                    XmlHelpers.Merge( ref parms, uriXml );
                else
                    parms = uriXml;
            }

            //merge parms
            if( HasValues )
            {
                XmlDocument values = new XmlDocument();
                values.LoadXml( Values.ToString() );

                if( parms != null )
                    XmlHelpers.Merge( ref parms, values );
                else
                    parms = values;
            }

            //kv_replace
            if( HasDynamic && parms != null )
                XmlHelpers.Merge( ref parms, Dynamic, _dynamicData );

            //expand ForEach variables
            if( HasForEach && parms != null )
                forEachParms = XmlHelpers.ExpandForEachAndApplyPatchValues( ref parms, ForEach );


            return parms;
        }

        Dictionary<object, object> ResolveYamlJson(ref List<object> forEachParms)
        {
            object parms = null;

            if( HasInheritedValues )
            {
                string tmp = YamlHelpers.Serialize( ((ParameterInfo)InheritedValues).Values );
                parms = YamlHelpers.Deserialize( tmp );
            }

            //make rest call
            if( HasUri )
            {
                string uriContent = WebRequestClient.GetString( Uri );

                if( parms != null )
                {
                    object values = YamlHelpers.Deserialize<object>( uriContent );

                    Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                    Dictionary<object, object> uv = (Dictionary<object, object>)values;
                    YamlHelpers.Merge( ref ip, uv );
                }
                else
                {
                    parms = YamlHelpers.Deserialize<object>( uriContent );
                }
            }

            if( parms == null )
                parms = new Dictionary<object, object>();
            Dictionary<object, object> p = (Dictionary<object, object>)parms;

            //merge parms
            if( HasValues && p != null )
                YamlHelpers.Merge( ref p, (Dictionary<object, object>)Values );

            //kv_replace
            if( HasDynamic && p != null )
                YamlHelpers.Merge( ref p, Dynamic, _dynamicData );

            //expand ForEach variables
            if( HasForEach && p != null )
                forEachParms = YamlHelpers.ExpandForEachAndApplyPatchValues( ref p, ForEach );


            return p;
        }

        string ResolveUnspecified()
        {
            StringBuilder sb = new StringBuilder();

            if( HasInheritedValues )
                sb.Append( InheritedValues.ToString() );

            //make rest call
            if( HasUri )
                sb.Append( WebRequestClient.GetString( Uri ) );

            //merge parms
            if( HasValues )
                sb.Append( Values.ToString() );

            if( HasDynamic )
                foreach( string key in _dynamicData.Keys )
                {
                    sb.Append( $"{key}:{_dynamicData[key]}," );
                }


            return sb.ToString().TrimEnd( ',' );
        }

        async Task<string> GetUri(string uri)
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync( uri );
        }

        public ParameterInfo GetCryptoValues(CryptoProvider planCrypto = null, bool isEncryptMode = true)
        {
            if( HasCrypto )
                Crypto.InheritSettingsIfRequired( planCrypto );

            switch( Type )
            {
                case SerializationType.Xml:
                {
                    XmlDocument values = new XmlDocument();
                    values.LoadXml( Values.ToString() );
                    ParameterInfo pi = XmlHelpers.GetCryptoValues( this, ref values, isEncryptMode );
                    pi.Values = XmlHelpers.Serialize<string>( values );
                    return pi;
                }
                case SerializationType.Yaml:
                case SerializationType.Json:
                {
                    return YamlHelpers.GetCryptoValues( this, isEncryptMode );
                }
                case SerializationType.Unspecified:
                default:
                {
                    return this;
                }
            }
        }
    }
}