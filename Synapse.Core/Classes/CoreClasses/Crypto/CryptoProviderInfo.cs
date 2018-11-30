
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class CryptoProviderInfo : ComponentInfoBase, ICloneable<CryptoProviderInfo>
    {
        public static readonly string DefaultType = "Synapse.Handlers.Crypto:RsaProvider";

        //setting the Order causes StartInfo to be serialized after base class properties
        [YamlMember( Order = 100 )]
        new public CryptoProviderStartInfo StartInfo
        {
            get { return base.StartInfo as CryptoProviderStartInfo; }
            set { base.StartInfo = value; }
        }

        public CryptoProviderInfo Clone(bool shallow = true)
        {
            return GetClone<CryptoProviderInfo>( shallow );
        }

        public static CryptoProviderInfo CreateSample()
        {
            return CreateSample<CryptoProviderInfo>( DefaultType );
        }
    }
}