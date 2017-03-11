﻿using System;
using System.Collections.Generic;

namespace Synapse.Core
{
    public class DynamicValue
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Option> Options { get; set; }

        public override string ToString()
        {
            return $"[{Name}]::[{Path}]";
        }
    }
    public class Option
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"[{Key}]::[{Value}]";
        }
    }
}