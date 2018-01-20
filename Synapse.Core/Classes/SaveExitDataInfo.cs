using System;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class SaveExitDataInfo
    {
        public string Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return !string.IsNullOrWhiteSpace( Config ); } }

        public string Parameters { get; set; }
        [YamlIgnore]
        public bool HasParaamters { get { return !string.IsNullOrWhiteSpace( Parameters ); } }

        public override string ToString()
        {
            return $"Config:[{Config}], Parameters:[{Parameters}]";
        }

        public static SaveExitDataInfo CreateSample()
        {
            return new SaveExitDataInfo()
            {
                Config = "ConfigSet Name",
                Parameters = "ParameterSet Name"
            };
        }

        public SaveExitDataInfo Clone()
        {
            return (SaveExitDataInfo)MemberwiseClone();
        }
    }
}