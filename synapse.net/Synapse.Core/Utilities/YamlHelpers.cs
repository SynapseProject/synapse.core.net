using System;
using System.IO;

using YamlDotNet.Serialization;

namespace Synapse.Core.Utilities
{
    public class YamlHelpers
    {
        public static T Deserialize<T>(TextReader reader, bool ignoreUnmatchedProperties = true)
        {
            DeserializerBuilder builder = new DeserializerBuilder();
            if( ignoreUnmatchedProperties )
                builder.IgnoreUnmatchedProperties();
            Deserializer deserializer = builder.Build();
            return deserializer.Deserialize<T>( reader );
        }

        public void Serialize(TextWriter tw, object data)
        {
            Serializer serializer = new Serializer();
            serializer.Serialize( tw, data );
        }

        public string Serialize(object data)
        {
            string result = null;
            using( StringWriter writer = new StringWriter() )
            {
                Serializer serializer = new Serializer();
                serializer.Serialize( writer, data );
                result = writer.ToString();
            }
            return result;
        }
    }
}