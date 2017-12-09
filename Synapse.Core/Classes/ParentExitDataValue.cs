using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ParentExitDataValue : IReplacementValueOptions
    {
        //public SerializationType SourceType { get; set; }
        public string TransformSource { get; set; }
        public string TransformDestination { get; set; }
        [YamlIgnore]
        public bool HasTransform { get { return !string.IsNullOrWhiteSpace( TransformSource ) && !string.IsNullOrWhiteSpace( TransformDestination ); } }

        string _source = null;
        public string Source
        {
            get
            {
                if( HasTransform && string.IsNullOrWhiteSpace( _source ) )
                    return TransformDestination;
                else
                    return _source;
            }
            set { _source = value; }
        }

        public string Destination { get; set; }
        public bool CastToForEachValues { get; set; }

        public bool Parse { get; set; }
        public string Replace { get; set; }
        public string Encode { get; set; }

        public override string ToString()
        {
            return $"[[{TransformSource}]::[{TransformDestination}]::[{Source}]::[{Destination}]::[{Replace}]::[{Encode}]::[{Parse}]";
        }


        public static ParentExitDataValue CreateSample()
        {
            ParentExitDataValue dv = new ParentExitDataValue()
            {
                TransformSource = "Element:IndexedElement[0]:Element",
                TransformDestination = "Element:IndexedElement[0]:Element",
                Source = "Element:IndexedElement[0]:Element",
                Destination = "Element:IndexedElement[0]:Element",
                Parse = true,
                Replace = "Regex expression",
                Encode = "Base64"
            };

            return dv;
        }
    }
}