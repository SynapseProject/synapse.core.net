using System;
using System.Collections.Generic;

namespace Synapse.Core
{
    public class DynamicValue : SourceTarget
    {
        public string Name { get; set; }
        public List<Option> Options { get; set; } = new List<Option>();

        public override string ToString()
        {
            return $"[Name:{Name}], Target:[{Target}], Parse:[{Parse}], Replace:[{Replace}], Encode:[{Encode}]";
        }


        new public static DynamicValue CreateSample()
        {
            DynamicValue dv = new DynamicValue()
            {
                Name = "URI parameter name",
                Target = "Element:IndexedElement[0]:Element",
                Parse = true,
                Encode = "None | Base64",
                Replace = "Regex Expression"
            };
            dv.Options.Add( Option.CreateSample() );
            dv.Options.Add( Option.CreateSample() );

            return dv;
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