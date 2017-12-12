using System;

namespace Synapse.Core
{
    public class ForEachValuesSource
    {
        public string From { get; set; }
        public string Source { get; set; }
        public bool Parse { get; set; }
        public string Destination { get; set; }


        public override string ToString()
        {
            return $"{From}::{Source}::{Parse}::{Destination}";
        }

        public static ForEachValuesSource CreateSample()
        {
            ForEachValuesSource fe = new ForEachValuesSource()
            {
                From = "Named ParameterInfo Block",
                Source = "Element:IndexedElement[0]:Element",
                Parse = true,
                Destination = "Element:IndexedElement[0]:Element"
            };

            return fe;
        }
    }
}