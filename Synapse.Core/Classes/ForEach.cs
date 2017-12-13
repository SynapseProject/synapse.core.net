using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ForEachInfo
    {
        public List<ForEachParameterSetSource> ParameterSetSources { get; set; }
        [YamlIgnore]
        public bool HasParameterSetSources { get { return ParameterSetSources != null && ParameterSetSources.Count > 0; } }

        public List<ForEachItem> Items { get; set; }
        [YamlIgnore]
        public bool HasItems { get { return Items != null && Items.Count > 0; } }

        [YamlIgnore]
        public bool HasContent { get { return HasParameterSetSources || HasItems; } }

        public static ForEachInfo CreateSample()
        {
            return new ForEachInfo()
            {
                ParameterSetSources = new List<ForEachParameterSetSource>() { ForEachParameterSetSource.CreateSample() },
                Items = new List<ForEachItem>() { ForEachItem.CreateSample() }
            };
        }
    }

    public class ForEachParameterSetSource
    {
        public string ParameterSet { get; set; }
        [YamlIgnore]
        public bool HasParameterSet { get { return !string.IsNullOrWhiteSpace( ParameterSet ); } }
        public string Source { get; set; }
        public bool Parse { get; set; }
        public string Destination { get; set; }


        public override string ToString()
        {
            return $"{ParameterSet}::{Source}::{Parse}::{Destination}";
        }

        public static ForEachParameterSetSource CreateSample()
        {
            ForEachParameterSetSource fe = new ForEachParameterSetSource()
            {
                ParameterSet = "Named ParameterInfo Block",
                Source = "Element:IndexedElement[0]:Element",
                Parse = true,
                Destination = "Element:IndexedElement[0]:Element"
            };

            return fe;
        }
    }

    public class ForEachItem
    {
        public string Path { get; set; }
        public List<object> Values { get; set; } = new List<object>();

        [YamlIgnore]
        public ForEachItem Child { get; set; }
        [YamlIgnore]
        public bool HasChild { get { return Child != null; } }

        [YamlIgnore]
        public Dictionary<object, object> PathAsPatchValues { get; set; }

        public override string ToString()
        {
            return Path;
        }

        public static ForEachItem CreateSample()
        {
            ForEachItem fe = new ForEachItem()
            {
                Path = "Element:IndexedElement[0]:Element"
            };
            fe.Values.AddRange( new string[] { "value0", "value1" } );

            return fe;
        }
    }
}