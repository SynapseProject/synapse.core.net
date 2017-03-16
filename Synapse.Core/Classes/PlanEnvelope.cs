using System;
using System.Collections.Generic;
using System.IO;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public class StartPlanEnvelope
    {
        public Plan Plan { get; set; }
        public Dictionary<string, string> DynamicParameters { get; set; }

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
}