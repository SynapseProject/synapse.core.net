using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.cli
{
    class Program
    {
        static bool _isSingleTaskModel = false;

        static void Main(string[] args)
        {
            int exitCode = 11;

            Arguments a = new Arguments( args );
            if( !a.IsParsed )
                WriteHelpAndExit( a.Message );

            if( a.Render != RenderAction.None )
            {
                if( a.Render == RenderAction.Encode )
                    Console.WriteLine( CryptoHelpers.Encode( a.Plan ) );
                else
                    Console.WriteLine( a.Plan );
            }
            else
            {
                ExecuteResult result = null;

                Plan plan = null;
                using( StringReader reader = new StringReader( a.Plan ) )
                    plan = Plan.FromYaml( reader );
                plan.Progress += plan_Progress;
                plan.LogMessage += plan_LogMessage;

                switch( a.TaskModel )
                {
                    case TaskModel.InProc:
                    {
                        Task t = Task.Run( () => result = plan.Start( a.Args, a.DryRun, inProc: true ) );
                        t.Wait();
                        break;
                    }
                    case TaskModel.External:
                    {
                        Task t = Task.Run( () => result = plan.Start( a.Args, a.DryRun, inProc: false ) );
                        t.Wait();
                        break;
                    }
                    case TaskModel.Single:
                    {
                        _isSingleTaskModel = true;
                        result = plan.ExecuteHandlerProcess_SingleAction( plan.Actions[0], a.Args, a.Data, a.DryRun );
                        break;
                    }
                }
                exitCode = (int)result.Status;
                //exitCode = (int)plan.Result.Status;
                if( a.TaskModel != TaskModel.Single )
                    File.WriteAllText( $"{plan.Name}.result.yml", plan.ResultPlan.ToYaml() );
            }

            //Console.WriteLine( $"exitCode:{exitCode}" );
            Environment.Exit( exitCode );
        }

        private static void plan_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            string msg = e.SerializeSimple();
            if( _isSingleTaskModel )
                msg = CryptoHelpers.Encode( msg );
            Console.WriteLine( msg );
        }

        private static void plan_LogMessage(object sender, LogMessageEventArgs e)
        {
            string msg = e.SerializeSimple();
            if( _isSingleTaskModel )
                msg = CryptoHelpers.Encode( msg );
            Console.WriteLine( msg );
        }


        #region Help
        static void WriteHelpAndExit(string errorMessage = null)
        {
            bool haveError = !string.IsNullOrWhiteSpace( errorMessage );

            ConsoleColor defaultColor = Console.ForegroundColor;

            Console_WriteLine( $"synapse.cli.exe, Version: {typeof( Program ).Assembly.GetName().Version}\r\n", ConsoleColor.Green );
            Console.WriteLine( "Syntax:" );
            Console_WriteLine( "  synapse.cli.exe /plan:{0}filePath{1}|{0}encodedPlanString{1} [/dryRun:true|false]", ConsoleColor.Cyan, "{", "}" );
            Console.WriteLine( "    [/taskModel:inProc|external] [/render:encode|decode] [dynamic parameters]\r\n" );
            Console_WriteLine( "  /plan{0,-8}- filePath: Valid path to plan file.", ConsoleColor.Green, "" );
            Console.WriteLine( "{0,-15}- encodedPlanString: Inline base64 encoded plan string.", "" );
            Console.WriteLine( "  /dryRun{0,-6}Specifies whether to execute the plan as a DryRun only.", "" );
            Console.WriteLine( "{0,-15}  Default is false.", "" );
            Console.WriteLine( "  /taskModel{0,-3}Specifies whether to execute the plan on an internal", "" );
            Console.WriteLine( "{0,-15}  thread or shell process.  Default is InProc.", "" );
            Console.WriteLine( "  /render{0,-6}- encode: Returns the base64 encoded value of the", "" );
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
        const string __data = "data";
        const string __dryrun = "dryrun";
        const string __taskmodel = "taskmodel";
        const string __render = "render";

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
                        if( CryptoHelpers.TryDecode( Args[__plan], out plan ) )
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

                #region Data
                if( Args.Keys.Contains( __data ) )
                {
                    Data = Args[__dryrun];
                    Args.Remove( __dryrun );
                }
                else
                {
                    Data = string.Empty;
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

                #region TaskModel
                if( Args.Keys.Contains( __taskmodel ) )
                {
                    TaskModel tm = TaskModel.InProc;
                    if( Enum.TryParse( Args[__taskmodel], true, out tm ) )
                        TaskModel = tm;
                    else
                        Message += "  * Unable to parse TaskModel value as 'inProc' or 'external'.\r\n";

                    Args.Remove( __taskmodel );
                }
                else
                {
                    TaskModel = TaskModel.InProc;
                }
                #endregion

                #region RenderAction
                if( Args.Keys.Contains( __render ) )
                {
                    RenderAction parse = RenderAction.None;
                    if( Enum.TryParse( Args[__render], true, out parse ) )
                        Render = parse;
                    else
                        Message += "  * Unable to parse Render value as 'encode' or 'decode'.\r\n";

                    Args.Remove( __render );
                }
                else
                {
                    Render = RenderAction.None;
                }
                #endregion
            }

            IsParsed &= string.IsNullOrWhiteSpace( Message );
        }

        public Dictionary<string, string> Args { get; internal set; }
        public string Plan { get; set; }
        public string Data { get; set; }
        public bool DryRun { get; set; }
        public TaskModel TaskModel { get; set; }
        public RenderAction Render { get; set; }
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

    enum TaskModel
    {
        InProc,
        External,
        Single
    }
    enum RenderAction
    {
        None,
        Encode,
        Decode
    }
}