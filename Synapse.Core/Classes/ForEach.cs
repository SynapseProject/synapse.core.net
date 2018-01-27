using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ForEachInfo : List<ForEachItem>
    {
        [YamlIgnore]
        public bool HasItems { get { return Count > 0; } }

        public List<ForEachItem> ParameterSourceItems { get; set; }
        [YamlIgnore]
        public bool HasParameterSourceItems
        {
            get
            {
                if( HasItems )
                    ParameterSourceItems = this.Where( fe => fe.HasParameterSource ).ToList();
                return ParameterSourceItems != null && ParameterSourceItems.Count > 0;
            }
        }

        public static ForEachInfo CreateSample()
        {
            return new ForEachInfo()
            {
                {  ForEachItem.CreateSample() }
            };
        }
    }

    public class ForEachItem : IReplacementValueOptions
    {
        public ParameterSourceInfo ParameterSource { get; set; }
        [YamlIgnore]
        public bool HasParameterSource { get { return ParameterSource != null; } }

        public string Target { get; set; }

        public string Replace { get; set; }
        [YamlIgnore]
        public bool HasReplace { get { return !string.IsNullOrWhiteSpace( Replace ); } }

        public EncodingType Encode { get; set; }
        [YamlIgnore]
        public bool IsBase64Encode { get { return Encode == EncodingType.Base64; } }

        public List<object> Values { get; set; } = new List<object>();

        [YamlIgnore]
        public ForEachItem Child { get; set; }
        [YamlIgnore]
        public bool HasChild { get { return Child != null; } }

        [YamlIgnore]
        public Dictionary<object, object> PathAsPatchValues { get; set; }

        public override string ToString()
        {
            return $"Target:[{Target}], Replace:[{Replace}], Replace:[{Replace}]";
        }

        public static ForEachItem CreateSample()
        {
            ForEachItem fe = new ForEachItem()
            {
                ParameterSource = ParameterSourceInfo.CreateSample(),
                Target = "Element:IndexedElement[0]:Element",
                Encode = EncodingType.Base64,
                Replace = "Regex Expression"
            };
            fe.Values.AddRange( new string[] { "value0", "value1" } );

            return fe;
        }
    }

    public class ParameterSourceInfo
    {
        public string Name { get; set; }
        [YamlIgnore]
        public bool HasName { get { return !string.IsNullOrWhiteSpace( Name ); } }
        [YamlIgnore]
        public bool IsNameParentExitData { get { return HasName && Name.Equals( "ParentExitData", StringComparison.OrdinalIgnoreCase ); } }

        public string Source { get; set; }
        public bool Parse { get; set; }


        public override string ToString()
        {
            return $"Name:[{Name}], Source:[{Source}], Parse:[{Parse}]";
        }

        public static ParameterSourceInfo CreateSample()
        {
            ParameterSourceInfo fe = new ParameterSourceInfo()
            {
                Name = "Named ParameterInfo Block",
                Source = "Element:IndexedElement[0]:Element"
            };

            return fe;
        }
    }
}