namespace Synapse.Core
{
    public interface IComponentInfo : ICloneable<IComponentInfo>
    {
        string Type { get; set; }
        ParameterInfo Config { get; set; }
        bool HasConfig { get; }

        IStartInfo StartInfo { get; set; }
    }
}