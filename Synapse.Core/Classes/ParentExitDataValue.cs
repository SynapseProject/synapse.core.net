using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ParentExitDataValue : IReplacementValueOptions
    {
        public string Source { get; set; }
        //public SerializationType SourceType { get; set; }
        public string Transform { get; set; }
        [YamlIgnore]
        public bool HasTransform { get { return !string.IsNullOrWhiteSpace( Transform ); } }
        public string Destination { get; set; }
        public bool Parse { get; set; }
        public string Replace { get; set; }
        public string Encode { get; set; }
        //public bool CastToForEachValues { get; set; }

        public override string ToString()
        {
            return $"[[{Source}]::[{Destination}]::[{Replace}]::[{Encode}]::[{Parse}]";
        }


        public static ParentExitDataValue CreateSample()
        {
            ParentExitDataValue dv = new ParentExitDataValue()
            {
                Source = "Element:IndexedElement[0]:Element",
                //SourceType = SerializationType.Yaml,
                Destination = "Element:IndexedElement[0]:Element",
                Parse = true,
                Replace = "Regex expression",
                Encode = "Base64"
            };

            return dv;
        }
    }
}