using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
	public class Config
	{
		public string Uri { get; set; }
		public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }
		public object Values { get; set; }
		public bool HasValues { get { return Values != null; } }
		public SerializationType Type { get; set; }

		public string Resolve()
		{
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
			return parms;
		}

		string ResolveXml()
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

			//todo: XmlSerializer
			return parms.ToString();
		}

		string ResolveJson()
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

			Dictionary<object, object> p = (Dictionary<object, object>)parms;

			//merge parms
			if( HasValues )
			{
				Utilities.MergeHelpers.MergeYaml( ref p, (Dictionary<object, object>)Values );
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

			return parms;
		}
	}
}