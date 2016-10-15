using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public partial class Plan
    {
        public Plan()
        {
            Actions = new List<ActionItem>();
        }

        public string Name { get; set; }
        public string UniqueName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public List<ActionItem> Actions { get; set; }
        public SecurityContext RunAs { get; set; }
        [YamlIgnore]
        public bool HasRunAs { get { return RunAs != null; } }
        public ExecuteResult Result { get; set; }

        public string ToYaml()
        {
            string yaml = string.Empty;
            using( StringWriter s = new StringWriter() )
            {
                Serializer serializer = new Serializer();
                serializer.Serialize( s, this );
                yaml = s.ToString();
            }
            return yaml;
        }

        public void ToYaml(TextWriter tw)
        {
            Serializer serializer = new Serializer();
            serializer.Serialize( tw, this );
        }

        public static Plan FromYaml(TextReader reader)
        {
            Deserializer deserializer = new Deserializer( ignoreUnmatched: false );
            return deserializer.Deserialize<Plan>( reader );
        }
    }
}