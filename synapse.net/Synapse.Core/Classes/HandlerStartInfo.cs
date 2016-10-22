using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Core
{
    public class HandlerStartInfo
    {
        public long InstanceId { get; set; }
        public string RequestUser { get; set; }
        public string RequestNumber { get; set; }
    }
}