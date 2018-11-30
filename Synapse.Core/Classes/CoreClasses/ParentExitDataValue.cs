using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ParentExitDataValue
    {
        public SourceTarget TransformInPlace { get; set; }
        [YamlIgnore]
        public bool HasTransformInPlace { get { return TransformInPlace != null; } }

        public SourceTarget CopyToValues { get; set; }
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


        public override string ToString()
        {
            return $"[TransformInPlace:[{TransformInPlace}], CopyToValues:[{CopyToValues}]";
        }


        public static ParentExitDataValue CreateSample()
        {
            ParentExitDataValue dv = new ParentExitDataValue()
            {
                TransformInPlace = SourceTarget.CreateSample(),
                CopyToValues = SourceTarget.CreateSample()
            };

            return dv;
        }
    }
}