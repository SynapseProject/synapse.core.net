using System;
using System.Collections.Generic;
using System.IO;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    public class PlanElementParms
    {
        public SerializationType Type { get; set; }
        public List<string> ElementPaths { get; set; }
    }
}