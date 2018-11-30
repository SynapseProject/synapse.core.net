using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public partial class ParameterInfo : IParameterInfo, ICloneable<ParameterInfo>
    {
        public ParameterInfo() { }

        public string Name { get; set; }
        [YamlIgnore]
        public bool HasName { get { return !string.IsNullOrWhiteSpace( Name ); } }

        public SerializationType Type { get; set; }


        public string InheritFrom { get; set; }
        [YamlIgnore]
        public bool HasInheritFrom { get { return !string.IsNullOrWhiteSpace( InheritFrom ); } }
        [YamlIgnore]
        public object InheritedValues { get; set; }
        [YamlIgnore]
        public bool HasInheritedValues { get { return InheritedValues != null; } }

        public string Uri { get; set; }
        [YamlIgnore]
        public bool HasUri { get { return !string.IsNullOrWhiteSpace( Uri ); } }

        public object Values { get; set; }
        [YamlIgnore]
        public bool HasValues { get { return Values != null; } }

        public List<DynamicValue> Dynamic { get; set; }
        [YamlIgnore]
        public bool HasDynamic { get { return Dynamic != null && Dynamic.Count > 0; } }

        public List<ParentExitDataValue> ParentExitData { get; set; }
        [YamlIgnore]
        public bool HasParentExitData { get { return ParentExitData != null && ParentExitData.Count > 0; } }

        public ForEachInfo ForEach { get; set; }
        [YamlIgnore]
        public bool HasForEach { get { return ForEach != null && ForEach.HasItems; } }


        public CryptoProvider Crypto { get; set; }
        [YamlIgnore]
        public bool HasCrypto { get { return Crypto != null; } }

        /// <summary>
        /// Serializes Valus to string, decrypting as required.
        /// </summary>
        /// <param name="planCrypto">Plan-level Crypto settings, to be inherited by local Crypto.</param>
        /// <returns>Yaml-/Json-/Xml-serialized values.</returns>
        public string GetSerializedValues(CryptoProvider planCrypto = null)
        {
            return GetSerializedValues( planCrypto, out string safeSerializedValues );
        }
        /// <summary>
        /// Serializes Valus to string, decrypting as required.  safeSerializedValues does not include decrypted values.
        /// </summary>
        /// <param name="planCrypto">Plan-level Crypto settings, to be inherited by local Crypto.</param>
        /// <param name="safeSerializedValues">Serialized Values, but with any Encrypted values still shown as encrypted.</param>
        /// <returns>Yaml-/Json-/Xml-serialized values.</returns>
        public string GetSerializedValues(CryptoProvider planCrypto, out string safeSerializedValues)
        {
            ParameterInfo pi = this;
            if( HasCrypto )
                pi = GetCryptoValues( planCrypto, isEncryptMode: false );

            string v = null;
            safeSerializedValues = null;
            switch( Type )
            {
                case SerializationType.Yaml:
                {
                    v = Utilities.YamlHelpers.Serialize( pi.Values );
                    safeSerializedValues = Utilities.YamlHelpers.Serialize( this.Values );
                    break;
                }
                case SerializationType.Json:
                {
                    v = Utilities.YamlHelpers.Serialize( pi.Values, serializeAsJson: true );
                    safeSerializedValues = Utilities.YamlHelpers.Serialize( this.Values, serializeAsJson: true );
                    break;
                }
                case SerializationType.Xml:
                {
                    v = Utilities.XmlHelpers.Serialize<object>( pi.Values );
                    safeSerializedValues = Utilities.XmlHelpers.Serialize<object>( this.Values );
                    break;
                }
                case SerializationType.Unspecified:
                {
                    v = pi.Values.ToString();
                    safeSerializedValues = this.Values.ToString();
                    break;
                }
            }

            return v;
        }

        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public ParameterInfo Clone(bool shallow = true)
        {
            return (ParameterInfo)MemberwiseClone();
        }


        public static ParameterInfo CreateSample()
        {
            ParameterInfo p = new ParameterInfo()
            {
                Name = "NameSupportsInheritance",
                Type = SerializationType.Yaml,
                Uri = "http://host/path",
                InheritFrom = "APrecedingNamedParamInfo",
                Crypto = CryptoProvider.CreateSample(),
                Values = "Custom values as defined by Handler/Provider"
            };
            p.Dynamic = new List<DynamicValue>
            {
                DynamicValue.CreateSample()
            };
            p.ParentExitData = new List<ParentExitDataValue>()
            {
                ParentExitDataValue.CreateSample()
            };
            p.ForEach = ForEachInfo.CreateSample();

            return p;
        }
    }
}