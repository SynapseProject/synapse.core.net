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
        [YamlIgnore]
        public bool HasReplace { get { return !string.IsNullOrWhiteSpace( Replace ); } }

        public EncodingType Encode { get; set; }
        [YamlIgnore]
        public bool IsBase64Encode { get { return Encode == EncodingType.Base64; } }


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
                Encode = EncodingType.Base64,
                Replace = "Regex Expression"
            };
        }
    }
}