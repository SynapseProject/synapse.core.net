using System;
using System.Collections.Generic;
using System.IO;

namespace Synapse.Common.Utilities
{
    public class AboutData
    {
        public object Config { get; set; }

        public List<FileData> Files { get; set; } = null;
        public string FilesCsv { get; set; } = null;

        public virtual void GetFiles(bool asCsv = false)
        {
            string currentPath = Path.GetDirectoryName( typeof( AboutData ).Assembly.Location );
            if( asCsv )
                FilesCsv = FileEnumerator.EnumerateFilesToCsv( currentPath );
            else
                Files = FileEnumerator.EnumerateFiles( currentPath );
        }
    }
}