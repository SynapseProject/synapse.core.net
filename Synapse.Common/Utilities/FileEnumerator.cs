using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Synapse.Common.Utilities
{
    public class FileEnumerator
    {
        public static List<FileData> EnumerateFiles(string directory)
        {
            List<FileData> list = new List<FileData>();

            DirectoryInfo dirInfo = new DirectoryInfo( directory );
            IEnumerable<FileInfo> files = dirInfo.EnumerateFiles( "*.*", SearchOption.AllDirectories );

            using( MD5 md5 = MD5.Create() )
                foreach( FileInfo fi in files )
                {
                    string hash = null;
                    try
                    {
                        using( FileStream stream = File.OpenRead( fi.FullName ) )
                            hash = BitConverter.ToString( md5.ComputeHash( stream ) ).Replace( "-", "" ).ToLower();
                    }
                    catch( Exception ex )
                    {
                        hash = ex.Message;
                    }

                    FileData fd = new FileData()
                    {
                        Name = fi.Name,
                        FullName = fi.FullName,
                        LastWriteTime = fi.LastWriteTime,
                        Length = fi.Length,
                        Hash = hash
                    };

                    string extension = fi.Extension.ToLower();
                    if( extension == ".exe" || extension == ".dll" )
                    {
                        FileVersionInfo v = FileVersionInfo.GetVersionInfo( fi.FullName );
                        fd.Version = new VersionData()
                        {
                            FileVersion = v.FileVersion,
                            FileDescription = v.FileDescription,
                            ProductName = v.ProductName,
                            ProductVersion = v.ProductVersion
                        };
                    }

                    list.Add( fd );
                }

            return list;
        }

        public static string EnumerateFilesToCsv(string directory)
        {
            StringBuilder list = new StringBuilder();

            DirectoryInfo dirInfo = new DirectoryInfo( directory );
            IEnumerable<FileInfo> files = dirInfo.EnumerateFiles( "*.*", SearchOption.AllDirectories );

            using( MD5 md5 = MD5.Create() )
                foreach( FileInfo fi in files )
                {
                    string hash = null;
                    try
                    {
                        using( FileStream stream = File.OpenRead( fi.FullName ) )
                            hash = BitConverter.ToString( md5.ComputeHash( stream ) ).Replace( "-", "" ).ToLower();
                    }
                    catch( Exception ex )
                    {
                        hash = ex.Message;
                    }

                    string outline = $"\"{fi.Name}\",\"{fi.FullName}\",\"{fi.LastWriteTime}\",\"{fi.Length}\",\"{hash}\"";

                    string extension = fi.Extension.ToLower();
                    if( extension == ".exe" || extension == ".dll" )
                    {
                        FileVersionInfo v = FileVersionInfo.GetVersionInfo( fi.FullName );
                        outline = $"{outline},\"{v.FileVersion}\",\"{v.FileDescription}\",\"{v.ProductName}\",\"{v.ProductVersion}\"";
                    }
                    else
                        outline = $"{outline},\"\",\"\",\"\",\"\"";

                    list.AppendLine( outline );
                }

            return list.ToString();
        }
    }
}