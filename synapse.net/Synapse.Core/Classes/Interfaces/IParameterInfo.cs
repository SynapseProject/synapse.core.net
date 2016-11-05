﻿using System;
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
}