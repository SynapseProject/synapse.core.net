using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Synapse.Core
{
    public class DynamicValue : SourceTarget
    {
        public List<Option> Options { get; set; } = new List<Option>();

        public TypeCode DataType { get; set; } = TypeCode.String;
        public string ValidationRegex { get; set; }
        public bool RestrictToOptions { get; set; } = true;

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
                Replace = "Regex Expression",
                DataType = TypeCode.String,
                ValidationRegex = "Regex Expression",
                RestrictToOptions = true
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
                DataType = DataType,
                ValidationRegex = ValidationRegex,
                RestrictToOptions = RestrictToOptions && Options?.Count > 0,

                Options = Options?.Count == 0 ? null : Options
            };
        }

        public bool Validate(string value, out string errorMessage)
        {
            errorMessage = null;
            bool ok = true;

            if( DataType != TypeCode.String )
            {
                try
                {
                    object x = Convert.ChangeType( value, DataType );
                }
                catch
                {
                    errorMessage = $"Could not convert DynamicValue [{value}] to [{DataType}] for parameter [{Source}].";
                    ok = false;
                }
            }

            if( !string.IsNullOrWhiteSpace( ValidationRegex ) )
            {
                if( !Regex.Match( value, ValidationRegex ).Success )
                {
                    errorMessage = $"DynamicValue [{value}] failed validation rule [{ValidationRegex}] for parameter [{Source}].";
                    ok = false;
                }
            }

            if( RestrictToOptions && Options?.Count > 0 )
            {
                bool found = false;
                foreach( Option option in Options )
                {
                    if( option.Value.Equals( value, StringComparison.OrdinalIgnoreCase ) )
                    {
                        found = true;
                        break;
                    }
                }

                if( !found )
                {
                    errorMessage = $"DynamicValue [{value}] failed validation rule [{nameof( RestrictToOptions )} for parameter [{Source}].";
                    ok = false;
                }
            }

            return ok;
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