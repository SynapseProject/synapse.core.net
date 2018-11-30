using System;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
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
            if( HasProvider )
                sc.Provider = Provider.Clone( shallow );
            if( HasParameters )
                sc.Parameters = Parameters.Clone( shallow );
            return sc;
        }


        public static SecurityContext CreateSample()
        {
            SecurityContext handler = new SecurityContext()
            {
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