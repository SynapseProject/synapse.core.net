using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ForEach
    {
        public string Path { get; set; }
        public List<string> Values { get; set; }

        [YamlIgnore]
        public ForEach Child { get; set; }
        [YamlIgnore]
        public bool HasChild { get { return Child != null; } }

        [YamlIgnore]
        public Dictionary<object, object> PathAsPatchValues { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}