using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
	public class DynamicValue
	{
		public string Name { get; set; }
		public string Path { get; set; }

		public override string ToString()
		{
			return string.Format( "[{0}]::[{1}]", Name, Path );
		}
	}

	public class Parameters
	{
		public string Uri { get; set; }
		public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }
		public object Values { get; set; }
		public bool HasValues { get { return Values != null; } }
		public List<DynamicValue> Dynamic { get; set; }
		public bool HasDynamic { get { return Dynamic != null && Dynamic.Count > 0; } }
		public SerializationType Type { get; set; }

		public string Resolve()
		{
			string parms = string.Empty;
			switch( Type )
			{
				case SerializationType.Xml:
				{
					parms = ResolveXmlParameters();
					break;
				}
				case SerializationType.Json:
				{
					parms = ResolveJsonParameters();
					break;
				}
				case SerializationType.Yaml:
				{
					parms = ResolveYamlParameters();
					break;
				}
				case SerializationType.Unspecified:
				{
					parms = ResolveUnspecifiedParameters();
					break;
				}
			}
			return parms;
		}

		string ResolveXmlParameters()
		{
			XmlDocument parms = null;

			//make rest call
			if( HasUri )
			{
				string uri = @"<xml attr='value0'><inner attri='foo001' /><data>foo0</data><data>foo0</data><inner attri='foo00' /></xml>";
				parms = new XmlDocument();
				parms.LoadXml( uri );
			}

			//merge parms
			if( HasValues )
			{
				if( parms != null )
				{
					XmlNode sourceNode = parms.ChildNodes[0];

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

			if( HasDynamic )
			{
				//kv_replace
			}

			//todo: XmlSerializer
			return parms.ToString();
		}

		string ResolveJsonParameters()
		{
			object parms = null;

			//make rest call
			if( HasUri )
			{
				string uri = "{  \"ApplicationName\": \"steve\",  \"EnvironmentName\": \"dev\",  \"Tier\": {    \"Name\": \"webserver\",    \"Type\": \"python\",    \"Version\": \"0.0\"  }}";
				using( StringReader sr = new StringReader( uri ) )
				{
					Deserializer deserializer = new Deserializer( ignoreUnmatched: true );
					parms = deserializer.Deserialize( sr );
				}
			}

			//merge parms
			if( HasValues )
			{
				Utilities.MergeHelpers.MergeYaml( ref parms, Values );
			}

			//kv_replace
			if( HasDynamic )
			{
				Utilities.MergeHelpers.MergeYaml( ref parms, Dynamic );
			}

			string p = null;
			using( StringWriter sw = new StringWriter() )
			{
				Serializer serializer = new Serializer();
				serializer.Serialize( sw, parms );
				p = sw.ToString();
			}

			return p;
		}

		string ResolveYamlParameters()
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

			//merge parms
			if( HasValues )
			{
				Utilities.MergeHelpers.MergeYaml( ref parms, Values );
			}

			//kv_replace
			if( HasDynamic )
			{
				Utilities.MergeHelpers.MergeYaml( ref parms, Dynamic );
			}

			string p = null;
			using( StringWriter sw = new StringWriter() )
			{
				Serializer serializer = new Serializer();
				serializer.Serialize( sw, parms );
				p = sw.ToString();
			}

			return p;
		}

		string ResolveUnspecifiedParameters()
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