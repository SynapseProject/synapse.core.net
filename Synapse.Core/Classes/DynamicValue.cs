﻿using System;
using System.Collections.Generic;

namespace Synapse.Core
{
    public class DynamicValue : SourceTarget
    {
        public List<Option> Options { get; set; } = new List<Option>();

        public override string ToString()
        {
            return $"[Name:{Source}], Target:[{Target}], Parse:[{Parse}], Replace:[{Replace}], Encode:[{Encode}]";
        }


        new public static DynamicValue CreateSample()
        {
            DynamicValue dv = new DynamicValue()
            {
                Description = "A human-friendly description.",
                Source = "URI parameter name",
                Target = "Element:IndexedElement[0]:Element",
                Parse = true,
                Encode = EncodingType.Base64,
                Replace = "Regex Expression"
            };
            dv.Options.Add( Option.CreateSample() );
            dv.Options.Add( Option.CreateSample() );

            return dv;
        }

        /// <summary>
        /// Sets nulls/defaults for non-discovery-based values.
        /// </summary>
        public DynamicValue AsSimpleValue()
        {
            return new DynamicValue
            {
                Description = Description,
                Source = Source,
                Target = null,
                Parse = false,
                Encode = EncodingType.None,
                Replace = null,

                Options = Options?.Count == 0 ? null : Options
            };
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

        public static Option CreateSample()
        {
            Option o = new Option()
            {
                Key = "key",
                Value = "value"
            };

            return o;
        }
    }
}