using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public interface IParameterInfo
    {
        string Name { get; set; }
        bool HasName { get; }
        SerializationType Type { get; set; }
        string InheritFrom { get; set; }
        bool HasInheritFrom { get; }
        object InheritedValues { get; set; }
        bool HasInheritedValues { get; }
        string Uri { get; set; }
        bool HasUri { get; }
        object Values { get; set; }
        bool HasValues { get; }
        List<DynamicValue> Dynamic { get; set; }
        bool HasDynamic { get; }
        object ResolvedValues { get; set; }
        string ResolvedValuesSerialized { get; set; }

        string Resolve(Dictionary<string, string> dynamicData = null);
    }

    public partial class ParameterInfo : IParameterInfo
    {
        public ParameterInfo() { }

        public string Name { get; set; }
        [YamlIgnore]
        public bool HasName { get { return !string.IsNullOrWhiteSpace( Name ); } }

        public SerializationType Type { get; set; }

        public string InheritFrom { get; set; }
        [YamlIgnore]
        public bool HasInheritFrom { get { return !string.IsNullOrWhiteSpace( InheritFrom ); } }
        [YamlIgnore]
        public object InheritedValues { get; set; }
        [YamlIgnore]
        public bool HasInheritedValues { get { return InheritedValues != null; } }

        public string Uri { get; set; }
        [YamlIgnore]
        public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }

        public object Values { get; set; }
        [YamlIgnore]
        public bool HasValues { get { return Values != null; } }

        public List<DynamicValue> Dynamic { get; set; }
        [YamlIgnore]
        public bool HasDynamic { get { return Dynamic != null && Dynamic.Count > 0; } }

        [YamlIgnore]
        public object ResolvedValues { get; set; }
        [YamlIgnore]
        public string ResolvedValuesSerialized { get; set; }
    }
}