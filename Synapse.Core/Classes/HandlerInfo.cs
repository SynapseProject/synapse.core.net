using System;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class HandlerInfo : ICloneable<HandlerInfo>
    {
        public string Type { get; set; }
        public ParameterInfo Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return Config != null; } }

        public HandlerStartInfoData StartInfo { get; set; }

        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public HandlerInfo Clone(bool shallow = true)
        {
            HandlerInfo handler = (HandlerInfo)MemberwiseClone();
            if( HasConfig )
                handler.Config = Config.Clone( shallow );
            return handler;
        }

        public override string ToString()
        {
            return Type;
        }


        public static HandlerInfo CreateSample()
        {
            HandlerInfo handler = new HandlerInfo()
            {
                Type = "Synapse.Handlers.CommandLine:CommandHandler",
                Config = ParameterInfo.CreateSample()
            };

            return handler;
        }
    }
}