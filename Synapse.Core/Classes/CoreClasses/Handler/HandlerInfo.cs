using System;

using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class HandlerInfo : ComponentInfoBase, ICloneable<HandlerInfo>
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
            IHandlerRuntime rt = Utilities.AssemblyLoader.Load( Type, defaultType );

            if( rt != null )
            {
                Type = rt.RuntimeType;
                rt.ActionName = actionName;

                string config = HasConfig ? Config.GetSerializedValues( planCrypto ) : null;
                rt.Initialize( config );
            }
            else
            {
                throw new Exception( $"Could not load {Type}." );
            }

            return rt;
        }
    }
}