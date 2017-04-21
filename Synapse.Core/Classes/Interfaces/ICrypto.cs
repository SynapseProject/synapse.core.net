using System;
using System.Collections.Generic;
using System.Threading;

namespace Synapse.Core
{
    public interface ICrypto
    {
        CryptoProvider Crypto { get; set; }
        bool HasCrypto { get; }
    }
}