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
            _dynamicData = dynamicData ?? new Dictionary<string, string>();
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
                    object values = null;
                    using( StringReader sr = new StringReader( uriContent ) )
                        values = YamlHelpers.Deserialize<object>( sr );

                    Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                    Dictionary<object, object> uv = (Dictionary<object, object>)values;
                    YamlHelpers.Merge( ref ip, uv );
                }
                else
                {
                    using( StringReader sr = new StringReader( uriContent ) )
                        parms = YamlHelpers.Deserialize<object>( sr );
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

        public ParameterInfo GetCryptoValues(bool isEncryptMode = true)
        {
            return YamlHelpers.GetCryptoValues( this, isEncryptMode );
        }

        public ParameterInfo GetCryptoValues_(bool isEncryptMode = true)
        {
            ParameterInfo result = null;

            if( HasCrypto )
            {
                Crypto.LoadRsaKeys();
                Crypto.IsEncryptMode = isEncryptMode;

                List<string> errors = new List<string>();
                string p = YamlHelpers.Serialize( this );
                Dictionary<object, object> source = YamlHelpers.Deserialize( p );
                foreach( string element in Crypto.Elements )
                {
                    try
                    {
                        Dictionary<object, object> patch = YamlHelpers.ConvertPathElementToDict( element );
                        YamlHelpers.HandleElementCrypto( source, patch, Crypto );
                    }
                    catch
                    {
                        errors.Add( element );
                    }
                }

                p = YamlHelpers.Serialize( source );
                result = YamlHelpers.Deserialize<ParameterInfo>( p );

                if( errors.Count == 0 )
                    result.Crypto.Errors = null;
                else
                    foreach( string error in errors )
                        result.Crypto.Errors.Add( error );
            }

            return result;
        }
    }
}