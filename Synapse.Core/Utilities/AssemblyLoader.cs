using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Core.Utilities
{
    public class AssemblyLoader
    {
        public IHandlerRuntime Load(string handlerType)
        {
            HashSet<string> knownFiles = new HashSet<string>();
            knownFiles.Add( "log4net" );
            knownFiles.Add( "microsoft.visualstudio.qualitytools.unittestframework" );
            knownFiles.Add( "nunit.framework" );
            knownFiles.Add( "sqlite.interop" );
            knownFiles.Add( "synapse.service.common" );
            knownFiles.Add( "synapse.unittests" );
            knownFiles.Add( "system.data.sqlite" );
            knownFiles.Add( "yamldotnet" );

            IHandlerRuntime hr = null;

            if( handlerType == null )
                handlerType = "Synapse.Handler.CommandLine:CommandLineHandler";
            else if( !handlerType.ToLower().EndsWith( "handler" ) )
                handlerType = $"{handlerType}Handler";

            if( handlerType.Contains( ":" ) )
            {
                string[] typeInfo = handlerType.Split( ':' );
                AssemblyName an = new AssemblyName( typeInfo[0] );
                Assembly hrAsm = Assembly.Load( an );

                Type handlerRuntime = hrAsm.GetType( typeInfo[1], true );
                hr = Activator.CreateInstance( handlerRuntime ) as IHandlerRuntime;
                hr.RuntimeType = handlerRuntime.AssemblyQualifiedName;
            }
            else
            {
                string currentDir = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
                DirectoryInfo dirInfo = new DirectoryInfo( currentDir );
                List<FileInfo> files = new List<FileInfo>(
                    dirInfo.EnumerateFiles( "*.dll", SearchOption.AllDirectories ) );

                IEnumerator<FileInfo> fileList = files.GetEnumerator();
                while( fileList.MoveNext() && hr == null )
                {
                    string current = fileList.Current.Name.ToLower().Replace( fileList.Current.Extension.ToLower(), string.Empty );
                    if( !knownFiles.Contains( current ) )
                    {
                        //assume that the name is complete, including namespace (if there is one)
                        try
                        {
                            AssemblyName an = new AssemblyName( current );
                            Assembly hrAsm = Assembly.Load( an );

                            Type handlerRuntime = hrAsm.GetType( handlerType, true );
                            hr = Activator.CreateInstance( handlerRuntime ) as IHandlerRuntime;
                            hr.RuntimeType = handlerRuntime.AssemblyQualifiedName;
                        }
                        catch
                        {
                            //probe all the Types, looking for partial match in name
                            try
                            {
                                AssemblyName an = new AssemblyName( current );
                                Assembly hrAsm = Assembly.Load( an );

                                string ht = handlerType.ToLower();
                                Type[] types = hrAsm.GetTypes();
                                foreach( Type t in types )
                                    if( t.GetInterfaces().Contains( typeof( IHandlerRuntime ) ) && t.Name.ToLower().Contains( ht ) )
                                    {
                                        hr = Activator.CreateInstance( t ) as IHandlerRuntime;
                                        hr.RuntimeType = t.AssemblyQualifiedName;
                                        break;
                                    }
                            }
                            catch { }
                        }
                    }
                }
            }

            return hr;
        }
    }
}