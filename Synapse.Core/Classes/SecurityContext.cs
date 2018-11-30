using System;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class SecurityContextProviderInfo : ComponentInfoBase, ICloneable<SecurityContextProviderInfo>
    {
        public static readonly string DefaultType = "Synapse.Handlers.SecurityContext:Win32Impersonator";

        new public SecurityContextProviderStartInfo StartInfo
        {
            get { return base.StartInfo as SecurityContextProviderStartInfo; }
            set { base.StartInfo = value; }
        }

        public SecurityContextProviderInfo Clone(bool shallow = true)
        {
            return GetClone<SecurityContextProviderInfo>( shallow );
        }

        public static SecurityContextProviderInfo CreateSample()
        {
            return CreateSample<SecurityContextProviderInfo>( DefaultType );
        }
    }

    public class SecurityContextProviderStartInfo : StartInfoBase
    {
    }

    public class SecurityContext : ICloneable<SecurityContext>, IInheritable
    {
        public void EnsureInitialized()
        {
            if( Provider == null )
                Provider = new SecurityContextProviderInfo();
        }

        public SecurityContextProviderInfo Provider { get; set; }
        [YamlIgnore]
        public bool HasProvider { get { return Provider != null; } }

        //public string Type { get; set; }
        //[YamlIgnore]
        //public bool HasType { get { return !string.IsNullOrWhiteSpace( Type ); } }

        //public ParameterInfo Config { get; set; }
        //[YamlIgnore]
        //public bool HasConfig { get { return Config != null; } }

        public ParameterInfo Parameters { get; set; }
        [YamlIgnore]
        public bool HasParameters { get { return Parameters != null; } }

        [YamlIgnore]
        public bool IsValid { get { return (HasProvider && (Provider.HasType || Provider.HasConfig)) || HasParameters; } }

        #region IInheritable
        /// <summary>
        /// Specifies if this SecurityContext block can be inherited by child SecurityContext blocks.
        /// </summary>
        public bool IsInheritable { get; set; } = true;
        /// <summary>
        /// Specifies if this SecurityContext instance will block inheritance from the parent SecurityContext instance.
        /// </summary>
        public bool BlockInheritance { get; set; }
        /// <summary>
        /// Indicates if the current settings are inherited from the parent SecurityContext block.
        /// </summary>
        public bool IsInherited { get; set; } = false;

        public void InheritSettingsIfAllowed(SecurityContext sourceContext)
        {
            if( sourceContext != null && sourceContext.IsInheritable && !this.BlockInheritance )
            {
                //Type = sourceContext.Type;
                //Config = sourceContext.Config?.Clone();
                Provider = sourceContext.Provider?.Clone();
                Parameters = sourceContext.Parameters?.Clone();
                IsInheritable = true;
                IsInherited = true;
            }
            else
            {
                IsInherited = false;  //overwrite any user-declared setting
            }
        }
        #endregion


        #region Clone, Sample
        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public SecurityContext Clone(bool shallow = true)
        {
            SecurityContext sc = (SecurityContext)MemberwiseClone();
            //if( HasConfig )
            //    sc.Config = Config.Clone( shallow );
            if( HasProvider )
                sc.Provider = Provider.Clone( shallow );
            if( HasParameters )
                sc.Parameters = Parameters.Clone( shallow );
            return sc;
        }

        //public override string ToString()
        //{
        //    return Type;
        //}


        public static SecurityContext CreateSample()
        {
            SecurityContext handler = new SecurityContext()
            {
                //Type = "Synapse.Handlers.SecurityContext:Win32Impersonator",
                //Config = ParameterInfo.CreateSample(),
                Provider = SecurityContextProviderInfo.CreateSample(),
                Parameters = ParameterInfo.CreateSample(),
                IsInheritable = true,
                BlockInheritance = true,
                IsInherited = true
            };

            return handler;
        }
        #endregion
    }
}