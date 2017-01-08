using System;
using System.IO;

using Synapse.Core.Utilities;


namespace Synapse.Service.Windows
{
    /// <summary>
    /// Hold the startup config for Synapse.Service; written as an independent class (not using .NET config) for cross-platform compatibility.
    /// </summary>
    public class SynapseServiceConfig
    {
        public SynapseServiceConfig()
        {
            Log4NetConversionPattern = "%d{ISO8601}|%-5p|(%t)|%m%n";
            SerializeResultPlan = true;
            ValidatePlanSignature = true;
        }

        public static readonly string CurrentPath = $"{Path.GetDirectoryName( typeof( SynapseServiceConfig ).Assembly.Location )}";
        public static readonly string FileName = $"{Path.GetDirectoryName( typeof( SynapseServiceConfig ).Assembly.Location )}\\Synapse.Service.config.yaml";

        public int MaxServerThreads { get; set; }
        public string AuditLogRootPath { get; set; }
        public string ServiceLogRootPath { get; set; }
        public string Log4NetConversionPattern { get; set; }
        public bool SerializeResultPlan { get; set; }
        public bool ValidatePlanSignature { get; set; }


        public void Serialize()
        {
            YamlHelpers.SerializeFile( FileName, this, serializeAsJson: false, emitDefaultValues: true );
        }

        public static SynapseServiceConfig Deserialze()
        {
            return YamlHelpers.DeserializeFile<SynapseServiceConfig>( FileName );
        }
    }
}