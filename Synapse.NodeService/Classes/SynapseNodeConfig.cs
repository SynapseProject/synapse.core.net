using System;
using System.IO;

using Synapse.Core.Utilities;


namespace Synapse.Services
{
    /// <summary>
    /// Hold the startup config for Synapse.Node; written as an independent class (not using .NET config) for cross-platform compatibility.
    /// </summary>
    public class SynapseNodeConfig
    {
        public SynapseNodeConfig()
        {
            Log4NetConversionPattern = "%d{ISO8601}|%-5p|(%t)|%m%n";
            SerializeResultPlan = true;
            ValidatePlanSignature = true;
        }

        public static readonly string CurrentPath = $"{Path.GetDirectoryName( typeof( SynapseNodeConfig ).Assembly.Location )}";
        public static readonly string FileName = $"{Path.GetDirectoryName( typeof( SynapseNodeConfig ).Assembly.Location )}\\Synapse.Node.config.yaml";

        public int MaxServerThreads { get; set; }
        public string AuditLogRootPath { get; set; }
        public string ServiceLogRootPath { get; set; }
        public string Log4NetConversionPattern { get; set; }
        public bool SerializeResultPlan { get; set; }
        public bool ValidatePlanSignature { get; set; }
        public string ControllerServiceUrl { get; set; }

        public string GetResolvedAuditLogRootPath()
        {
            if( Path.IsPathRooted( AuditLogRootPath ) )
                return AuditLogRootPath;
            else
                return PathCombine( CurrentPath, AuditLogRootPath );
        }

        public string GetResolvedServiceLogRootPath()
        {
            if( Path.IsPathRooted( ServiceLogRootPath ) )
                return ServiceLogRootPath;
            else
                return PathCombine( CurrentPath, ServiceLogRootPath );
        }

        /// <summary>
        /// A wrapper on Path.Combine to correct for fronting/trailing backslashes that otherwise fail in Path.Combine.
        /// </summary>
        /// <param name="paths">An array of parts of the path.</param>
        /// <returns>The combined path</returns>
        public static string PathCombine(params string[] paths)
        {
            if( paths.Length > 0 )
            {
                int last = paths.Length - 1;
                for( int c = 0; c <= last; c++ )
                {
                    if( c != 0 )
                    {
                        paths[c] = paths[c].Trim( Path.DirectorySeparatorChar );
                    }
                    if( c != last )
                    {
                        paths[c] = string.Format( "{0}\\", paths[c] );
                    }
                }
            }
            else
            {
                return string.Empty;
            }

            return Path.Combine( paths );
        }


        public void Serialize()
        {
            YamlHelpers.SerializeFile( FileName, this, serializeAsJson: false, emitDefaultValues: true );
        }

        public static SynapseNodeConfig Deserialze()
        {
            return YamlHelpers.DeserializeFile<SynapseNodeConfig>( FileName );
        }
    }
}