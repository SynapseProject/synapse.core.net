using System;

namespace Synapse.Common.Utilities
{
    public class VersionData
    {
        public string FileVersion { get; set; }
        public string FileDescription { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }

        public override string ToString()
        {
            return $"\"{FileVersion}\",\"{FileDescription}\",\"{ProductName}\",\"{ProductVersion}\""; ;
        }
    }
}