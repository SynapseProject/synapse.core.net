using System;
using System.Collections.Generic;
using System.IO;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public class StartPlanEnvelope
    {
        public Plan Plan { get; set; }
        //public List<KeyValueItem> DynamicParameters { get; set; } = new List<KeyValueItem>();
        public Dictionary<string, string> DynamicParameters { get; set; }

        //public void SetDynamicParameters(Dictionary<string, string> dynamicParameters)
        //{
        //    foreach( KeyValuePair<string, string> kvp in dynamicParameters )
        //        DynamicParameters.Add( new KeyValueItem( kvp ) );
        //}
        //public Dictionary<string, string> GetDynamicParameters()
        //{
        //    Dictionary<string, string> dynamicParameters = new Dictionary<string, string>();
        //    foreach( KeyValueItem kvi in DynamicParameters )
        //        dynamicParameters.Add( kvi.Key, kvi.Value );

        //    return dynamicParameters;
        //}


        public string ToYaml(bool encode = false)
        {
            string yaml = null;

            using( StringWriter s = new StringWriter() )
                yaml = YamlHelpers.Serialize( this );

            if( encode )
                yaml = CryptoHelpers.Encode( yaml );

            return yaml;
        }

        public static StartPlanEnvelope FromYaml(string yaml, bool isEncoded = false)
        {
            if( isEncoded )
                yaml = CryptoHelpers.Decode( yaml );

            return YamlHelpers.Deserialize<StartPlanEnvelope>( yaml );
        }
    }

    //public class KeyValueItem
    //{
    //    public KeyValueItem() { }
    //    public KeyValueItem(KeyValuePair<string, string> kvp)
    //    {
    //        Key = kvp.Key;
    //        Value = kvp.Value;
    //    }

    //    public string Key { get; set; }
    //    public string Value { get; set; }
    //}
}