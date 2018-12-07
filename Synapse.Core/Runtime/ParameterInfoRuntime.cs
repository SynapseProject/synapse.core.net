using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public partial class ParameterInfo : IParameterInfo
    {
        const string __nodata = "Unable to capture data for error message.";
        Dictionary<string, string> _dynamicData = null;

        public object Resolve(out List<object> forEachParms, Dictionary<string, string> dynamicData = null, object parentExitData = null,
            Dictionary<string, ParameterInfo> globalParamSets = null)
        {
            _dynamicData = dynamicData ?? new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
            forEachParms = new List<object>();

            object parms = null;
            switch( Type )
            {
                case SerializationType.Xml:
                {
                    parms = ResolveXml( ref forEachParms, parentExitData, globalParamSets );
                    break;
                }
                case SerializationType.Yaml:
                case SerializationType.Json:
                {
                    parms = ResolveYamlJson( ref forEachParms, parentExitData, globalParamSets );
                    break;
                }
                case SerializationType.Unspecified:
                {
                    parms = ResolveUnspecified( parentExitData );
                    break;
                }
            }

            if( forEachParms.Count == 0 )
                forEachParms.Add( parms );

            Values = parms;

            return parms;
        }

        XmlDocument ResolveXml(ref List<object> forEachParms, object parentExitData, Dictionary<string, ParameterInfo> globalParamSets)
        {
            string context = "ResolveXml";
            string errData = __nodata;

            try
            {
                XmlDocument parms = null;

                if( HasInheritedValues )
                {
                    context = "InheritedValues=>Clone";
                    errData = InheritedValues.GetType().ToString();
                    parms = (XmlDocument)((XmlDocument)((ParameterInfo)InheritedValues).Values).Clone();
                }

                if( HasUri )
                {
                    context = "Uri=>Fetch";
                    try { errData = new Uri( Uri ).ToString(); } catch { errData = Uri.ToString(); }

                    string uriContent = WebRequestClient.GetString( Uri );
                    context = "Uri=>Fetch, LoadXml";
                    errData = uriContent;
                    XmlDocument uriXml = new XmlDocument();
                    uriXml.LoadXml( uriContent );

                    context = parms != null ? "Merge->Inherited" : "Assign to parms";
                    context = $"Uri=>{context}";
                    errData = XmlHelpers.Serialize<XmlDocument>( uriXml );
                    if( parms != null )
                        XmlHelpers.Merge( ref parms, uriXml );
                    else
                        parms = uriXml;
                }

                //merge parms
                if( HasValues )
                {
                    context = "HasValues=>LoadXml";
                    errData = Values.ToString();
                    XmlDocument values = new XmlDocument();
                    values.LoadXml( Values.ToString() );

                    context = parms != null ? "Merge->Inherited+Uri+Values" : "Assign to parms";
                    context = $"Uri=>{context}";
                    errData = XmlHelpers.Serialize<XmlDocument>( values );
                    if( parms != null )
                        XmlHelpers.Merge( ref parms, values );
                    else
                        parms = values;
                }


                if( parms == null )
                    parms = new XmlDocument();
                
                
                //kv_replace
                if( HasDynamic )
                {
                    context = "HasDynamic=>Merge->Inherited+Uri+Values+Dynamic";
                    try { errData = YamlHelpers.Serialize( _dynamicData ); } catch { errData = __nodata; } //YamlHelpers not a mistake, used it on purpose for easy to read error data
                    XmlHelpers.Merge( ref parms, Dynamic, _dynamicData );
                }

                if( HasParentExitData && parentExitData != null )
                {
                    context = "ParentExitData=>Init Xml Source";
                    errData = parentExitData.GetType().ToString();

                    XmlDocument xd = new XmlDocument();
                    if( parentExitData is XmlNode )
                        xd.InnerXml = ((XmlNode)parentExitData).OuterXml;
                    else if( parentExitData is XmlNode[] )
                        xd.InnerXml = ((XmlNode[])parentExitData)[0].OuterXml;
                    else if( parentExitData is string )
                        xd.InnerXml = (string)parentExitData;
                    else
                        xd = (XmlDocument)parentExitData;

                    context = "ParentExitData=>Merge->Inhetited+Uri+Values+Dynamic+ParentExitData";
                    errData = XmlHelpers.Serialize<XmlDocument>( xd );
                    XmlHelpers.Merge( ref parms, ParentExitData, ref xd );
                }

                if( HasForEach && parms != null )
                {
                    //assemble ForEach variables
                    if( ForEach.HasParameterSourceItems )
                    {
                        context = "ForEach=>HasParameterSourceItems";
                        errData = null;
                        XmlHelpers.SelectForEachFromValues( ForEach.ParameterSourceItems, ref parms, globalParamSets, parentExitData );
                    }

                    //expand ForEach variables
                    context = "ForEach=>ExpandForEach";
                    try { errData = XmlHelpers.Serialize<XmlDocument>( parms ); } catch { errData = __nodata; }
                    forEachParms = XmlHelpers.ExpandForEachAndApplyPatchValues( ref parms, ForEach );
                }


                return parms;
            }
            catch( Exception ex )
            {
                throw new Exception( GetResolveExceptionMessage( context, errData ), ex );
            }
        }


        Dictionary<object, object> ResolveYamlJson(ref List<object> forEachParms, object parentExitData, Dictionary<string, ParameterInfo> globalParamSets)
        {
            string context = "ResolveYamlJson";
            string errData = __nodata;

            try
            {
                object parms = null;

                if( HasInheritedValues )
                {
                    context = "InheritedValues=>Serialize";
                    string tmp = YamlHelpers.Serialize( ((ParameterInfo)InheritedValues).Values );
                    errData = tmp;
                    context = "InheritedValues=>Deserialize";
                    parms = YamlHelpers.Deserialize( tmp );
                }


                //make rest call
                if( HasUri )
                {
                    context = "Uri=>Fetch";
                    try { errData = new Uri( Uri ).ToString(); } catch { errData = Uri.ToString(); }

                    string uriContent = WebRequestClient.GetString( Uri );
                    errData = uriContent;

                    if( parms != null )
                    {
                        context = "Uri=>Desrialize, Merge->Inherited+Uri";
                        object values = YamlHelpers.Deserialize<object>( uriContent );

                        Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                        Dictionary<object, object> uv = (Dictionary<object, object>)values;
                        YamlHelpers.Merge( ref ip, uv );
                    }
                    else
                    {
                        context = "Uri=>Desrialize only";
                        parms = YamlHelpers.Deserialize<object>( uriContent );
                    }
                }


                if( parms == null )
                    parms = new Dictionary<object, object>();


                context = "Parms=>Cast to Dictionary";
                errData = parms.GetType().ToString();
                Dictionary<object, object> p = (Dictionary<object, object>)parms;


                //merge parms
                if( HasValues && p != null )
                {
                    context = "HasValues=>Merge->Inhetited+Uri+Values";
                    try { errData = YamlHelpers.Serialize( p ); } catch { errData = __nodata; }
                    YamlHelpers.Merge( ref p, (Dictionary<object, object>)Values );
                }


                //kv_replace
                if( HasDynamic && p != null )
                {
                    context = "HasDynamic=>Merge->Inhetited+Uri+Values+Dynamic";
                    try { errData = YamlHelpers.Serialize( _dynamicData ); } catch { errData = __nodata; }
                    YamlHelpers.Merge( ref p, Dynamic, _dynamicData );
                }


                if( HasParentExitData && p != null && parentExitData != null )
                {
                    context = "ParentExitData=>Serialize";
                    errData = __nodata;

                    string xd = parentExitData is string ? parentExitData.ToString() : YamlHelpers.Serialize( parentExitData );
                    errData = xd;
                    context = "ParentExitData=>Deserialize";
                    object values = YamlHelpers.Deserialize( xd );
                    //select the exact values

                    context = "ParentExitData=>Merge->Inhetited+Uri+Values+Dynamic+ParentExitData";
                    Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                    Dictionary<object, object> pv = (Dictionary<object, object>)values;
                    YamlHelpers.Merge( ref ip, ParentExitData, ref pv );
                }


                if( HasForEach && p != null )
                {
                    //assemble ForEach variables
                    if( ForEach.HasParameterSourceItems )
                    {
                        context = "ForEach=>HasParameterSourceItems";
                        errData = null;
                        YamlHelpers.SelectForEachFromValues( ForEach.ParameterSourceItems, ref p, globalParamSets, parentExitData );
                    }

                    //expand ForEach variables
                    context = "ForEach=>ExpandForEach";
                    try { errData = YamlHelpers.Serialize( p ); } catch { errData = __nodata; }
                    forEachParms = YamlHelpers.ExpandForEachAndApplyPatchValues( ref p, ForEach );
                }


                return p;
            }
            catch( Exception ex )
            {
                throw new Exception( GetResolveExceptionMessage( context, errData ), ex );
            }
        }

        string GetResolveExceptionMessage(string context, string data)
        {
            string b64 = null;
            if( !string.IsNullOrWhiteSpace( data ) && data != __nodata )
                try { b64 = $", Data encoded as Base64: [{CryptoHelpers.Encode( data )}]"; }
                catch { }
            return $"Exception resolving ParameterInfo! Context: [{context}]. Data: [{data}]{b64}";
        }

        string ResolveUnspecified(object parentExitData)
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

            if( HasParentExitData )
                sb.Append( parentExitData.ToString() );

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