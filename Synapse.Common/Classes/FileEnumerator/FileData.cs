using System;

namespace Synapse.Common.Utilities
{
    public class FileData
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Length { get; set; }
        public string Hash { get; set; }
        public VersionData Version { get; set; } = null;

        public override string ToString()
        {
            string version = Version != null ? Version.ToString() : "\"\",\"\",\"\",\"\"";
            return $"\"{Name}\",\"{FullName}\",\"{LastWriteTime}\",\"{Length}\",\"{Hash}\",{version}";
        }
    }
}
