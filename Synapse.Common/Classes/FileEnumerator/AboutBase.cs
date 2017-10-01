using System;
using System.Collections.Generic;

namespace Synapse.Common.Utilities
{
    public abstract class AboutBase
    {
        public List<FileData> Files { get; set; } = null;
        public string FilesCsv { get; set; } = null;

        public abstract void GetFiles(bool asCsv = false);
    }
}