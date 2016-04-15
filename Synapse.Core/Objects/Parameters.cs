using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{

	public class Parameters
	{
		public string Uri { get; set; }
		public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }
		public object Values { get; set; }
		public bool HasValues { get { return Values != null; } }
		public SerializationType Type { get; set; }

		public string Resolve()
		{
			string parms = string.Empty;
			if( HasValues )
			{
				switch(Type)
				{
					case SerializationType.Xml:
					{
						break;
					}
					case SerializationType.Json:
					{
						break;
					}
					case SerializationType.Yaml:
					{
						using( StringWriter sw = new StringWriter() )
						{
							Serializer serializer = new Serializer();
							serializer.Serialize( sw, Values );
							parms = sw.ToString();
						}
						break;
					}
					case SerializationType.Unspecified:
					{
						break;
					}
				}
			}

			if( HasUri )
			{
				parms = string.Empty; //make rest call
				//merge parms
				switch( Type )
				{
					case SerializationType.Xml:
					{
						break;
					}
					case SerializationType.Json:
					{
						break;
					}
					case SerializationType.Yaml:
					{
						break;
					}
					case SerializationType.Unspecified:
					{
						break;
					}
				}
			}
			return parms;
		}
	}
}