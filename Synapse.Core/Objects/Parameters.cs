using System;
using System.IO;
using System.Xml;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
	public class Parameters
	{
		public string Uri { get; set; }
		public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }
		public object Values { get; set; }
		public bool HasValues { get { return Values != null; } }
		public string Dynamic { get; set; }
		public bool HasDynamic { get { return !string.IsNullOrWhiteSpace( Dynamic ); } }
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
			string parms = string.Empty;

			XmlDocument xml = null;

			if( HasUri )
			{
				xml = new XmlDocument();
				xml.LoadXml( @"<xml attr='value1'><data>foo1</data><inner attri='foo' /></xml>" );

				parms = string.Empty; //make rest call
			}

			if( HasValues )
			{
				if( xml != null )
				{
					XmlNode sourceNode = xml.ChildNodes[0];

					XmlDocument config = new XmlDocument();
					config.LoadXml( Values.ToString() );

					Utilities.MergeHelpers.MergeXml( ref xml, config );

					Utilities.MergeHelpers.MergeXml( ref sourceNode, config.ChildNodes[0] );
				}
				else
				{
					xml = new XmlDocument();
					xml.LoadXml( Values.ToString() );
				}
			}

			if( HasDynamic )
			{
				//kv_replace
			}

			return parms;
		}

		string ResolveJsonParameters()
		{
			string parms = string.Empty;

			if( HasValues )
			{
			}

			if( HasUri )
			{
				parms = string.Empty; //make rest call
				//merge parms
			}

			if( HasDynamic )
			{
				//kv_replace
			}

			return parms;
		}

		string ResolveYamlParameters()
		{
			string parms = string.Empty;

			if( HasValues )
			{
				using( StringWriter sw = new StringWriter() )
				{
					Serializer serializer = new Serializer();
					serializer.Serialize( sw, Values );
					parms = sw.ToString();
				}
			}

			if( HasUri )
			{
				parms = string.Empty; //make rest call
				//merge parms
			}

			if( HasDynamic )
			{
				//kv_replace
			}

			return parms;
		}

		string ResolveUnspecifiedParameters()
		{
			string parms = string.Empty;

			if( HasValues )
			{
			}

			if( HasUri )
			{
				parms = string.Empty; //make rest call
				//merge parms
			}

			if( HasDynamic )
			{
				//kv_replace
			}

			return parms;
		}
	}
}