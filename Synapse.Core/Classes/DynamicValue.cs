using System;
using System.Collections.Generic;

namespace Synapse.Core
{
    public class DynamicValue
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Replace { get; set; }
        public string Encode { get; set; }
        public List<Option> Options { get; set; } = new List<Option>();

        public override string ToString()
        {
            return $"[{Name}]::[{Path}]::[{Replace}]::[{Encode}]";
        }


        public static DynamicValue CreateSample()
        {
            DynamicValue dv = new DynamicValue()
            {
                Name = "URI parameter name",
                Path = "Element:IndexedElement[0]:Element",
                Replace = "Regex expression",
                Encode = "Base64"
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