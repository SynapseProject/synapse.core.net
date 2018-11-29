using System;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public abstract class ComponentInfoBase : IComponentInfo
    {
        public string Type { get; set; }
        [YamlIgnore]
        public bool HasType { get { return !string.IsNullOrWhiteSpace( Type ); } }

        public ParameterInfo Config { get; set; }
        [YamlIgnore]
        public bool HasConfig { get { return Config != null; } }

        IStartInfo IComponentInfo.StartInfo { get; set; }

        object ICloneable.Clone() { return GetClone<IComponentInfo>( true ); }
        IComponentInfo ICloneable<IComponentInfo>.Clone(bool shallow) { return GetClone<IComponentInfo>( true ); }
        protected T GetClone<T>(bool shallow = true) where T : class, IComponentInfo
        {
            T clone = MemberwiseClone() as T;
            if( HasConfig )
                clone.Config = Config.Clone( shallow );
            return clone;
        }

        protected static T CreateSample<T>(string exampleType) where T : class, IComponentInfo, new()
        {
            T sample = new T()
            {
                Type = exampleType,
                Config = ParameterInfo.CreateSample()
            };

            return sample;
        }


        public override string ToString()
        {
            return Type;
        }
    }
}