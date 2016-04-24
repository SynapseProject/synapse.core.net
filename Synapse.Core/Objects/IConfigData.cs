using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
	interface IConfigData
	{
		string Name { get; set; }
		bool HasName { get; }
		SerializationType Type { get; set; }
		string InheritFrom { get; set; }
		bool HasInheritFrom { get; }
		object InheritedValues { get; set; }
		bool HasInheritedValues { get; }
		string Uri { get; set; }
		bool HasUri { get; }
		object Values { get; set; }
		bool HasValues { get; }
		List<DynamicValue> Dynamic { get; set; }
		bool HasDynamic { get; }
		string ResolvedValues { get; set; }

		string Resolve(Dictionary<string, string> dynamicParameters = null);
	}

	public abstract class ConfigDataBase
	{
		#region properties
		public string Name { get; set; }
		[YamlIgnore]
		public bool HasName { get { return !string.IsNullOrWhiteSpace( Name ); } }

		public SerializationType Type { get; set; }

		public string InheritFrom { get; set; }
		[YamlIgnore]
		public bool HasInheritFrom { get { return !string.IsNullOrWhiteSpace( InheritFrom ); } }
		[YamlIgnore]
		public object InheritedValues { get; set; }
		[YamlIgnore]
		public bool HasInheritedValues { get { return InheritedValues != null; } }

		public string Uri { get; set; }
		[YamlIgnore]
		public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }

		public object Values { get; set; }
		[YamlIgnore]
		public bool HasValues { get { return Values != null; } }

		public List<DynamicValue> Dynamic { get; set; }
		[YamlIgnore]
		public bool HasDynamic { get { return Dynamic != null && Dynamic.Count > 0; } }

		[YamlIgnore]
		public string ResolvedValues { get; set; }
		#endregion


		private Dictionary<string, string> _dynamicParameters = null;

		public string Resolve(Dictionary<string, string> dynamicParameters = null)
		{
			_dynamicParameters = dynamicParameters ?? new Dictionary<string, string>();

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

			ResolvedValues = parms;
			return parms;
		}

		string ResolveXml()
		{
			XmlDocument parms = null;

			if( HasInheritedValues )
			{
				parms = new XmlDocument();
				parms.LoadXml( InheritedValues.ToString() );
			}

			//make rest call
			if( HasUri )
			{
				if( parms != null )
				{
					XmlDocument values = new XmlDocument();
					values.LoadXml( InheritedValues.ToString() );

					Utilities.MergeHelpers.MergeXml( ref parms, values );
				}
				else
				{
					string uri = @"<xml attr='value0'><inner attri='foo001' /><data>foo0</data><data>foo0</data><inner attri='foo00' /></xml>";
					parms = new XmlDocument();
					parms.LoadXml( uri );
				}
			}

			//merge parms
			if( HasValues )
			{
				if( parms != null )
				{
					XmlDocument values = new XmlDocument();
					values.LoadXml( Values.ToString() );

					Utilities.MergeHelpers.MergeXml( ref parms, values );
				}
				else
				{
					parms = new XmlDocument();
					parms.LoadXml( Values.ToString() );
				}
			}

			//kv_replace
			if( HasDynamic )
			{
				Utilities.MergeHelpers.MergeXml( ref parms, Dynamic, _dynamicParameters );
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
				if( parms != null )
				{
					object values = null;
					string uri = "{  \"ApplicationName\": \"steve\",  \"EnvironmentName\": \"dev\",  \"Tier\": {    \"Name\": \"webserver\",    \"Type\": \"python\",    \"Version\": \"0.0\"  }}";
					using( StringReader sr = new StringReader( uri ) )
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
					string uri = "{  \"ApplicationName\": \"steve\",  \"EnvironmentName\": \"dev\",  \"Tier\": {    \"Name\": \"webserver\",    \"Type\": \"python\",    \"Version\": \"0.0\"  }}";
					using( StringReader sr = new StringReader( uri ) )
					{
						Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
						parms = deserializer.Deserialize( sr );
					}
				}
			}

			Dictionary<object, object> p = (Dictionary<object, object>)parms;

			//merge parms
			if( HasValues )
			{
				Utilities.MergeHelpers.MergeYaml( ref p, (Dictionary<object, object>)Values );
			}

			//kv_replace
			if( HasDynamic )
			{
				Utilities.MergeHelpers.MergeYaml( ref p, Dynamic, _dynamicParameters );
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
				string uri =
@"Magical: Mystery
Lucy: In the sky
Kitten:
  Cat: Tommy
  Color: Rat";
				using( StringReader sr = new StringReader( uri ) )
				{
					Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
					parms = deserializer.Deserialize( sr );
				}
			}

			Dictionary<object, object> p = (Dictionary<object, object>)parms;

			//merge parms
			if( HasValues )
			{
				Utilities.MergeHelpers.MergeYaml( ref p, (Dictionary<object, object>)Values );
			}

			//kv_replace
			if( HasDynamic )
			{
				Utilities.MergeHelpers.MergeYaml( ref p, Dynamic, _dynamicParameters );
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
	}
}