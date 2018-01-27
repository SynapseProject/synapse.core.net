using System;

namespace Synapse.Core
{
    public interface IReplacementValueOptions
    {
        string Replace { get; set; }
        bool HasReplace { get; }
        EncodingType Encode { get; set; }
        bool IsBase64Encode { get; }
    }
}