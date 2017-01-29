﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class HandlerInfo : ICloneable<HandlerInfo>
    {
        public string Type { get; set; }
        public ParameterInfo Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return Config != null; } }

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
    }
}