using System;

namespace Synapse.Core
{
    public interface ICloneable<T> : ICloneable
    {
        T Clone(bool shallow = true);
    }
}