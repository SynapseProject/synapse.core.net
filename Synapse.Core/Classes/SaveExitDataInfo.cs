using System;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class SaveExitDataInfo
    {
        public string Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return !string.IsNullOrWhiteSpace( Config ); } }

        public string Paraamters { get; set; }
        [YamlIgnore]
        public bool HasParaamters { get { return !string.IsNullOrWhiteSpace( Paraamters ); } }

        public override string ToString()
        {
            return $"Source:[{Config}], Target:[{Paraamters}]";
        }

        public static SaveExitDataInfo CreateSample()
        {
            return new SaveExitDataInfo()
            {
                Config = "ConfigSet Name",
                Paraamters = "ParameterSet Name"
            };
        }

        public SaveExitDataInfo Clone()
        {
            return (SaveExitDataInfo)MemberwiseClone();
        }
    }
}