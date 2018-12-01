using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class HandlerInfo : ComponentInfoBase, ICloneable<HandlerInfo>, IRuntimeComponentCreator<IHandlerRuntime>
    {
        public static readonly string DefaultType = "Synapse.Handlers.CommandLine:CommandHandler";

        //setting the Order causes StartInfo to be serialized after base class properties
        [YamlMember( Order = 100 )]
        new public HandlerStartInfoData StartInfo
        {
            get { return base.StartInfo as HandlerStartInfoData; }
            set { base.StartInfo = value; }
        }

        public HandlerInfo Clone(bool shallow = true)
        {
            return GetClone<HandlerInfo>( shallow );
        }

        public static HandlerInfo CreateSample()
        {
            return CreateSample<HandlerInfo>( DefaultType );
        }


        public IHandlerRuntime CreateRuntime(string planDefaultHandlerType, CryptoProvider planCrypto, string actionName)
        {
            string defaultType = !string.IsNullOrWhiteSpace( planDefaultHandlerType ) ? planDefaultHandlerType : DefaultType;
            return CreateRuntime<IHandlerRuntime>( defaultType, planCrypto, actionName );
        }
    }
}