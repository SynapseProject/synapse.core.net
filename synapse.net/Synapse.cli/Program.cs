using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Arguments a = new Arguments( args );
            if( !a.IsParsed )
                WriteHelpAndExit( a.Message );

            if( a.Encode != EncodeAction.None )
            {
                if( a.Encode == EncodeAction.Encode )
                    Console.WriteLine( CrytoHelpers.Encode( a.Plan ) );
                else
                    Console.WriteLine( a.Plan );
            }
            else
            {
                if( a.InProc )
                {
                    Plan plan = null;
                    plan.Progress += plan_Progress;
                    using( StringReader reader = new StringReader( a.Plan ) )
                        plan = Plan.FromYaml( reader );
                    Task t = Task.Run( () => plan.Start( a.Args, a.DryRun ) );
                    t.Wait();
                }
                else
                {
                    Console.WriteLine( $"Execute Plan with dryrun:{a.DryRun},  inproc:{a.InProc}\r\nParams:" );
                    foreach( string key in a.Args.Keys )
                    {
                        Console.WriteLine( $"Key: {key}, Value: {a.Args[key]}" );
                    }
                    Console.WriteLine( a.Plan );
                }
            }

            Environment.Exit( 0 );
        }

        private static void plan_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            throw new NotImplementedException();
        }


        #region Help
        static void WriteHelpAndExit(string errorMessage = null)
        {
            bool haveError = !string.IsNullOrWhiteSpace( errorMessage );

            ConsoleColor defaultColor = Console.ForegroundColor;

            Console_WriteLine( $"synapse.cli.exe, Version: {typeof( Program ).Assembly.GetName().Version}\r\n", ConsoleColor.Green );
            Console.WriteLine( "Syntax:" );
            Console_WriteLine( "  synapse.cli.exe /plan:{0}filePath{1}|{0}encodedPlanString{1} [/dryRun:true|false]", ConsoleColor.Cyan, "{", "}" );
            Console.WriteLine( "    [/inproc:true|false] [/encode:encode|decode] [dynamic parameters]\r\n" );
            Console_WriteLine( "  /plan{0,-8}- filePath: Valid path to plan file.", ConsoleColor.Green, "" );
            Console.WriteLine( "{0,-15}- encodedPlanString: Inline base64 encoded plan string.", "" );
            Console.WriteLine( "  /dryRun{0,-6}Specifies whether to execute the plan as a DryRun only.", "" );
            Console.WriteLine( "{0,-15}  Default is false.", "" );
            Console.WriteLine( "  /inproc{0,-6}Specifies whether to execute the plan on an internal", "" );
            Console.WriteLine( "{0,-15}  thread.  Default is true.", "" );
            Console.WriteLine( "  /encode{0,-6}- encode: Returns the base64 encoded value of the", "" );
            Console.WriteLine( "{0,-15}  specifed plan file.", "" );
            Console.WriteLine( "{0,-15}- decode: Returns the base64 decoded value of the specified", "" );
            Console.WriteLine( "{0,-15}  encodedPlanString.", "" );
            Console.WriteLine( "  dynamic{0,-6}Any remaining /arg:value pairs will passed to the plan", "" );
            Console.WriteLine( "{0,-15}  as dynamic parms.", "" );

            if( haveError )
                Console_WriteLine( $"\r\n\r\n*** Last error:\r\n{errorMessage}\r\n", ConsoleColor.Red );

            Console.ForegroundColor = defaultColor;

            Environment.Exit( haveError ? 1 : 0 );
        }

        static void Console_WriteLine(string s, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine( s, args );
        }
        #endregion
    }

    internal class Arguments
    {
        const string __plan = "plan";
        const string __dryrun = "dryrun";
        const string __inproc = "inproc";
        const string __encode = "encode";

        public Arguments(string[] args)
        {
            IsParsed = true;

            if( args.Length == 0 )
            {
                IsParsed = false;
            }
            else if( args.Length == 1 )
            {
                string p = args[0].ToLower();
                if( p.Equals( "/?" ) || p.Equals( "/help" ) )
                    IsParsed = false;
            }

            if( IsParsed )
            {
                Args = ParseCmdLine( args );

                #region Plan
                if( Args.Keys.Contains( __plan ) )
                {
                    if( File.Exists( Args[__plan] ) )
                    {
                        Plan = File.ReadAllText( Args[__plan] );
                    }
                    else
                    {
                        string plan = null;
                        if( CrytoHelpers.TryDecode( Args[__plan], out plan ) )
                            Plan = plan;
                        else
                            Message = "  * Unable to resolve Plan as path or encoded string.\r\n";
                    }

                    Args.Remove( __plan );
                }
                else
                {
                    Message = "No plan specified.";
                }
                #endregion

                #region DryRun
                if( Args.Keys.Contains( __dryrun ) )
                {
                    bool dryrun = false;
                    if( bool.TryParse( Args[__dryrun], out dryrun ) )
                        DryRun = dryrun;
                    else
                        Message += "  * Unable to parse DryRun value as bool.\r\n";

                    Args.Remove( __dryrun );
                }
                else
                {
                    DryRun = false;
                }
                #endregion

                #region InProc
                if( Args.Keys.Contains( __inproc ) )
                {
                    bool inproc = false;
                    if( bool.TryParse( Args[__inproc], out inproc ) )
                        InProc = inproc;
                    else
                        Message += "  * Unable to parse InProc value as bool.\r\n";

                    Args.Remove( __inproc );
                }
                else
                {
                    InProc = true;
                }
                #endregion

                #region EncodeAction
                if( Args.Keys.Contains( __encode ) )
                {
                    EncodeAction parse = EncodeAction.None;
                    if( Enum.TryParse<EncodeAction>( Args[__encode], true, out parse ) )
                        Encode = parse;
                    else
                        Message += "  * Unable to parse Encode value as 'encode' or 'decode'.\r\n";

                    Args.Remove( __encode );
                }
                else
                {
                    Encode = EncodeAction.None;
                }
                #endregion
            }

            IsParsed &= string.IsNullOrWhiteSpace( Message );
        }

        public Dictionary<string, string> Args { get; internal set; }
        public string Plan { get; set; }
        public bool DryRun { get; set; }
        public bool InProc { get; set; }
        public EncodeAction Encode { get; set; }
        public string Message { get; internal set; }
        public bool IsParsed { get; internal set; }

        Dictionary<string, string> ParseCmdLine(string[] args)
        {
            IsParsed = true;
            Dictionary<string, string> options = new Dictionary<string, string>();

            string pattern = @"(?<argname>/\w+):(?<argvalue>.*)";
            for( int i = 0; i < args.Length; i++ )
            {
                Match match = Regex.Match( args[i], pattern );

                // If match not found, command line args are improperly formed.
                if( match.Success )
                {
                    options[match.Groups["argname"].Value.ToLower().TrimStart( '/' )] =
                        match.Groups["argvalue"].Value;
                }
                else
                {
                    Message = "The command line arguments are not valid or are improperly formed. Use 'argname:argvalue' for extended arguments.\r\n";
                    IsParsed = false;
                    break;
                }
            }

            return options;
        }
    }

    class KeyComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x.Equals( y, StringComparison.OrdinalIgnoreCase );
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    enum EncodeAction
    {
        None,
        Encode,
        Decode
    }
}