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
        const string __nodata = "[Unable to capture data for error message.]";
        private Dictionary<string, string> _dynamicData = null;

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
            if( HasDynamic )
            {
                if( parms == null )
                    parms = new XmlDocument();
                XmlHelpers.Merge( ref parms, Dynamic, _dynamicData );
            }

            if( HasParentExitData && parentExitData != null )
            {
                if( parms == null )
                    parms = new XmlDocument();
                XmlDocument pea = new XmlDocument();
                if( parentExitData is XmlNode )
                    pea.InnerXml = ((XmlNode)parentExitData).OuterXml;
                else if( parentExitData is XmlNode[] )
                    pea.InnerXml = ((XmlNode[])parentExitData)[0].OuterXml;
                else if( parentExitData is string )
                    pea.InnerXml = (string)parentExitData;
                else
                    pea = (XmlDocument)parentExitData;

                XmlHelpers.Merge( ref parms, ParentExitData, ref pea );
            }

            if( HasForEach && parms != null )
            {
                //assemble ForEach variables
                if( ForEach.HasParameterSourceItems )
                    XmlHelpers.SelectForEachFromValues( ForEach.ParameterSourceItems, ref parms, globalParamSets, parentExitData );

                //expand ForEach variables
                forEachParms = XmlHelpers.ExpandForEachAndApplyPatchValues( ref parms, ForEach );
            }


            return parms;
        }


        Dictionary<object, object> ResolveYamlJson(ref List<object> forEachParms, object parentExitData, Dictionary<string, ParameterInfo> globalParamSets)
        {
            object parms = null;

            try
            {
                if( HasInheritedValues )
                {
                    string tmp = YamlHelpers.Serialize( ((ParameterInfo)InheritedValues).Values );
                    parms = YamlHelpers.Deserialize( tmp );
                }
            }
            catch( Exception ex )
            {
                throw new Exception( $"Could not process inherited values.", ex );
            }

            //make rest call
            if( HasUri )
            {
                string ud = Uri.ToString();

                try
                {
                    string uriContent = WebRequestClient.GetString( Uri );
                    ud = uriContent;

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
                catch( Exception ex )
                {
                    throw new Exception( GetResolveExceptionMessage( "Uri", ud ), ex );
                }
            }


            if( parms == null )
                parms = new Dictionary<object, object>();


            Dictionary<object, object> p = null;
            try
            {
                p = (Dictionary<object, object>)parms;
            }
            catch( Exception ex )
            {
                throw new Exception( GetResolveExceptionMessage( "CastParmsToDictionary", parms.GetType().ToString() ), ex );
            }


            try
            {
                //merge parms
                if( HasValues && p != null )
                    YamlHelpers.Merge( ref p, (Dictionary<object, object>)Values );
            }
            catch( Exception ex )
            {
                throw new Exception( GetResolveExceptionMessage( "Merge:Inhetited+Uri=>Values", parms.GetType().ToString() ), ex );
            }


            try
            {
                //kv_replace
                if( HasDynamic && p != null )
                    YamlHelpers.Merge( ref p, Dynamic, _dynamicData );
            }
            catch( Exception ex )
            {
                string dd = __nodata;
                try { dd = YamlHelpers.Serialize( _dynamicData ); } catch { }
                throw new Exception( GetResolveExceptionMessage( "Merge:Dynamic", dd ), ex );
            }


            if( HasParentExitData && p != null && parentExitData != null )
            {
                string xd = __nodata;

                try
                {
                    xd = parentExitData is string ? parentExitData.ToString() : YamlHelpers.Serialize( parentExitData );
                    xd = YamlHelpers.Serialize( parentExitData );
                    object values = YamlHelpers.Deserialize( xd );
                    //select the exact values

                    Dictionary<object, object> ip = (Dictionary<object, object>)parms;
                    Dictionary<object, object> pv = (Dictionary<object, object>)values;
                    YamlHelpers.Merge( ref ip, ParentExitData, ref pv );
                }
                catch( Exception ex )
                {
                    throw new Exception( GetResolveExceptionMessage( "ParentExitData", xd ), ex );
                }
            }


            if( HasForEach && p != null )
            {
                //assemble ForEach variables
                if( ForEach.HasParameterSourceItems )
                    try
                    {
                        YamlHelpers.SelectForEachFromValues( ForEach.ParameterSourceItems, ref p, globalParamSets, parentExitData );
                    }
                    catch( Exception ex )
                    {
                        throw new Exception( GetResolveExceptionMessage( "ForEach:HasParameterSourceItems", null ), ex );
                    }

                //expand ForEach variables
                try
                {
                    forEachParms = YamlHelpers.ExpandForEachAndApplyPatchValues( ref p, ForEach );
                }
                catch( Exception ex )
                {
                    string pd = __nodata;
                    try { pd = YamlHelpers.Serialize( p ); } catch { }
                    throw new Exception( GetResolveExceptionMessage( "ExpandForEach", pd ), ex );
                }
            }


            return p;
        }

        string GetResolveExceptionMessage(string context, string data)
        {
            string b64 = null;
            if( !string.IsNullOrWhiteSpace( data ) && data != __nodata )
                try { b64 = $", Data as Base64: [{CryptoHelpers.Encode( data )}]"; }
                catch { }
            return $"Could not process {context} values. Data: [{data}]{b64}";
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