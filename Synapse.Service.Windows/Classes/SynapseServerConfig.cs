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
        public static readonly string CurrentPath = $"{Path.GetDirectoryName( typeof( SynapseServiceConfig ).Assembly.Location )}";
        public static readonly string FileName = $"{Path.GetDirectoryName( typeof( SynapseServiceConfig ).Assembly.Location )}\\Synapse.Service.config.yaml";

        public int MaxServerThreads { get; set; }
        public string LogRootPath { get; set; }
        public string Log4NetConversionPattern { get; set; }

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