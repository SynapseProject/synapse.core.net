using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ForEach
    {
        public string Path { get; set; }
        public List<string> Values { get; set; } = new List<string>();

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

        public static ForEach CreateSample()
        {
            ForEach fe = new ForEach()
            {
                Path = "Element:IndexedElement[0]:Element"
            };
            fe.Values.AddRange( new string[] { "value0", "value1" } );

            return fe;
        }
    }
}