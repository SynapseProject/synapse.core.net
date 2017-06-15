using System;

namespace Synapse.Core
{
	public enum SerializationType
	{
        Yaml,
        Xml,
		Json,
        Html,
		Unspecified
	}

    public struct SerializationContentType
    {
        public const string Yaml = "application/yaml";
        public const string Xml = "application/xml";
        public const string Json = "application/json";
        public const string Html = "text/html";
        public const string Unspecified = "text/plain";

        public static string GetContentType(SerializationType serializationType)
        {
            switch(serializationType)
            {
                case SerializationType.Xml: { return Xml; }
                case SerializationType.Json: { return Json; }
                case SerializationType.Html: { return Html; }
                case SerializationType.Unspecified: { return Unspecified; }
                default: { return Yaml; }
            }
        }
    }
}