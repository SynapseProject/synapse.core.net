using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ForEachInfo
    {
        public List<ForEachParameterSource> ParameterSources { get; set; }
        [YamlIgnore]
        public bool HasParameterSources { get { return ParameterSources != null && ParameterSources.Count > 0; } }

        public List<ForEachItem> Items { get; set; }
        [YamlIgnore]
        public bool HasItems { get { return Items != null && Items.Count > 0; } }

        [YamlIgnore]
        public bool HasContent { get { return HasParameterSources || HasItems; } }

        public static ForEachInfo CreateSample()
        {
            return new ForEachInfo()
            {
                ParameterSources = new List<ForEachParameterSource>() { ForEachParameterSource.CreateSample() },
                Items = new List<ForEachItem>() { ForEachItem.CreateSample() }
            };
        }
    }

    public class ForEachParameterSource : SourceTargetBase
    {
        public string Name { get; set; }
        [YamlIgnore]
        public bool HasName { get { return !string.IsNullOrWhiteSpace( Name ); } }
        [YamlIgnore]
        public bool IsNameParentExitData { get { return HasName && Name.Equals( "ParentExitData", StringComparison.OrdinalIgnoreCase ); } }


        public ForEachItem ToForEachItem()
        {
            return new ForEachItem()
            {
                Target = Target,
                Replace = Replace,
                Encode = Encode
            };
        }


        public override string ToString()
        {
            return $"Name:[{Name}], Source:[{Source}], Target:[{Target}], Parse:[{Parse}]";
        }

        new public static ForEachParameterSource CreateSample()
        {
            ForEachParameterSource fe = new ForEachParameterSource()
            {
                Name = "Named ParameterInfo Block",
                Source = "Element:IndexedElement[0]:Element",
                Parse = true,
                Target = "Element:IndexedElement[0]:Element",
                Encode = "None | Base64",
                Replace = "Regex Expression"
            };

            return fe;
        }
    }

    public class ForEachItem : IReplacementValueOptions
    {
        public string Target { get; set; }
        public string Replace { get; set; }
        public string Encode { get; set; }
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
                Target = "Element:IndexedElement[0]:Element",
                Encode = "None | Base64",
                Replace = "Regex Expression"
            };
            fe.Values.AddRange( new string[] { "value0", "value1" } );

            return fe;
        }
    }
}