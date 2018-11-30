
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class SecurityContextProviderInfo : ComponentInfoBase, ICloneable<SecurityContextProviderInfo>
    {
        public static readonly string DefaultType = "Synapse.Handlers.SecurityContext:Win32Impersonator";

        //setting the Order causes StartInfo to be serialized after base class properties
        [YamlMember( Order = 100 )]
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
}