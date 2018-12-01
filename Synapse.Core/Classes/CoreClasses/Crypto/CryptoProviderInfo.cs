using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class CryptoProviderInfo : ComponentInfoBase, ICloneable<CryptoProviderInfo>, IRuntimeComponentCreator<ICryptoRuntime>
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


        public ICryptoRuntime CreateRuntime(string planDefaultCryptoType, CryptoProvider planCrypto, string actionName)
        {
            string defaultType = !string.IsNullOrWhiteSpace( planDefaultCryptoType ) ? planDefaultCryptoType : DefaultType;
            return CreateRuntime<ICryptoRuntime>( defaultType, planCrypto, actionName );
        }
    }
}