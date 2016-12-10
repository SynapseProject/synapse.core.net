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

        List<ForEach> ForEach { get; set; }
        bool HasForEach { get; }

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

        string GetSerializedValues();

        object Resolve(out List<object> forEachParms, Dictionary<string, string> dynamicData = null);
    }
}