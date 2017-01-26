using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json;


namespace Synapse.Common.CmdLine
{
    public abstract class HttpApiCliBase
    {
        protected virtual void RunMethod(object instance, string methodName, string[] args)
        {
            bool needHelp = args.Length == 2 && args[1].ToLower().Contains( "help" );

            MethodInfo mi = instance.GetType().GetMethod( methodName );
            ParameterInfo[] parms = mi.GetParameters();

            if( needHelp )
            {
                Console.WriteLine( $"Parameter options for {methodName}:\r\n" );
                WriteMethodParametersHelp( parms );
            }
            else
            {
                List<object> parameters = new List<object>();
                if( parms.Length > 0 )
                    parameters = GetMethodParameters( args, 1, parms, 0 );

                try
                {
                    object result = mi.Invoke( instance, parameters.ToArray() );
                    if( result is IList && ((IList)result).Count == 1 )
                    {
                        result = ((IList)result)[0];
                    }

                    string jsonString = JsonConvert.SerializeObject( result, Formatting.Indented );
                    Console.WriteLine( jsonString );
                }
                catch( Exception ex )
                {
                    WriteException( ex );
                }
            }
        }

        #region utility methods
        protected virtual List<object> GetMethodParameters(string[] args, int cmdlineStartIndex, ParameterInfo[] parms, int parmsStartIndex)
        {
            if( args.Length < (cmdlineStartIndex + 1) )
                WriteHelpAndExit( "Not enough arguments specified." );

            Dictionary<string, string> options = ParseCmdLine( args, cmdlineStartIndex );

            List<object> parameters = new List<object>();
            for( int i = parmsStartIndex; i < parms.Length; i++ )
            {
                ParameterInfo parm = parms[i];
                if( options.Keys.Contains( parm.Name.ToLower() ) )
                    parameters.Add( ParseInput( options[parm.Name.ToLower()], parm.ParameterType ) );
                else
                    parameters.Add( null );
            }
            return parameters;
        }

        protected virtual Dictionary<string, string> ParseCmdLine(string[] args, int startIndex)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();

            string pattern = @"(?<argname>\w+):(?<argvalue>.*)";
            for( int i = startIndex; i < args.Length; i++ )
            {
                Match match = Regex.Match( args[i], pattern );

                // If match not found, command line args are improperly formed.
                if( match.Success )
                    options[match.Groups["argname"].Value.ToLower()] = match.Groups["argvalue"].Value.ToLower();
                else
                    WriteHelpAndExit( "The command line arguments are not valid or are improperly formed. Use 'argname:argvalue' for extended arguments." );
            }

            return options;
        }

        protected virtual object ParseInput(string input, Type type)
        {
            if( type == typeof( List<Guid> ) )
            {
                return null; // input.CsvToList<Guid>();
            }
            else if( type == typeof( Guid? ) || type == typeof( Guid ) )
            {
                return Guid.Parse( input );
            }
            else if( type == typeof( int? ) || type == typeof( int ) )
            {
                return int.Parse( input );
            }
            else if( type == typeof( bool? ) || type == typeof( bool ) )
            {
                return bool.Parse( input );
            }
            else if( type == typeof( DateTime? ) || type == typeof( DateTime ) )
            {
                return bool.Parse( input );
            }
            else if( type.IsEnum )
            {
                return Enum.Parse( type, input, true );
            }
            else
            {
                return input;
            }
        }

        protected virtual void WriteMethodParametersHelp(ParameterInfo[] parms, int startIndex = 0, string prefix = null)
        {
            int count = 0;
            for( int i = startIndex; i < parms.Length; i++ )
            {
                count++;
                ParameterInfo parm = parms[i];
                Console.WriteLine( "\t{0,-30}{1}", parm.Name, GetTypeFriendlyName( parm.ParameterType, prefix ) );
            }
            if( count == 0 )
                Console.WriteLine( $"\tNo additional parameter options." );
        }

        protected virtual string GetTypeFriendlyName(Type type, string prefix)
        {
            string typeName = type.ToString().ToLower();
            if( typeName.Contains( "guid" ) )
            {
                if( typeName.Contains( "generic.list" ) )
                    return "Csv list of Guids or JSON list of Guids";
                else
                    return "Guid";
            }
            else if( typeName.Contains( "int" ) )
            {
                return "int";
            }
            else if( typeName.Contains( "bool" ) )
            {
                return "bool";
            }
            else if( typeName.Contains( "string" ) )
            {
                return "string";
            }
            else if( typeName.Contains( "datetime" ) )
            {
                return "DateTime";
            }
            else if( type.IsEnum )
            {
                return GetEnumValuesCsv( type );
            }
            else
            {
                return type.ToString().Replace( prefix, "" );
            }
        }

        protected virtual string GetEnumValuesCsv(Type enumType)
        {
            Array values = Enum.GetValues( enumType );
            List<object> av = new List<object>();
            foreach( object v in values ) av.Add( v );
            return string.Join( ",", av );
        }

        protected virtual void WriteException(Exception ex)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( $"\r\n*** An error occurred:\r\n" );
            string[] msgs = WebApi.Utilities.UnwindException( ex ).Split( new string[] { @"\r\n" }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string msg in msgs )
                Console.WriteLine( msg );
            Console.WriteLine( $"\r\n" );
            Console.ForegroundColor = currentForeground;
        }
        #endregion


        protected virtual void Console_WriteLine(string s, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine( s, args );
        }

        protected abstract void WriteHelpAndExit(string s);
    }
}