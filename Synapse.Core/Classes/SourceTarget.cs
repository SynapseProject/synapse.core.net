using System;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class SourceTarget : IReplacementValueOptions
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public bool Parse { get; set; }
        public string Replace { get; set; }
        public string Encode { get; set; }

        public override string ToString()
        {
            return $"Source:[{Source}], Target:[{Target}], Parse:[{Parse}], Replace:[{Replace}], Encode:[{Encode}]";
        }

        public static SourceTarget CreateSample()
        {
            return new SourceTarget()
            {
                Source = "Element:IndexedElement[0]:Element",
                Target = "Element:IndexedElement[0]:Element",
                Parse = true,
                Encode = "None | Base64",
                Replace = "Regex Expression"
            };
        }
    }
}