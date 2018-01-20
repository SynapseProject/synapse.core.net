using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ParentExitDataValue
    {
        public SourceTargetBase TransformInPlace { get; set; }
        [YamlIgnore]
        public bool HasTransformInPlace { get { return TransformInPlace != null; } }

        public SourceTargetBase CopyToValues { get; set; }
        [YamlIgnore]
        public bool HasCopyToValues { get { return CopyToValues != null; } }

        [YamlIgnore]
        public string Source
        {
            get
            {
                if( HasCopyToValues && !string.IsNullOrWhiteSpace( CopyToValues.Source ) )
                    return CopyToValues.Source;
                else if( HasTransformInPlace && !string.IsNullOrWhiteSpace( TransformInPlace.Source ) )
                    return TransformInPlace.Target;
                else
                    return null;
            }
        }



        //public bool CastToForEachItems { get; set; }


        public override string ToString()
        {
            return $"[TransformInPlace:[{TransformInPlace}], CopyToValues:[{CopyToValues}]";
        }


        public static ParentExitDataValue CreateSample()
        {
            ParentExitDataValue dv = new ParentExitDataValue()
            {
                TransformInPlace = SourceTargetBase.CreateSample(),
                CopyToValues = SourceTargetBase.CreateSample()
            };

            return dv;
        }
    }

    public class SourceTargetBase : IReplacementValueOptions
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

        public static SourceTargetBase CreateSample()
        {
            return new SourceTargetBase()
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