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

            EnsureDatabaseExists();

            Arguments a = new Arguments( args );
            if( !a.IsParsed )
                WriteHelpAndExit( a.Message );

            if( a.IsCrypto )
            {
                if( a.IsEncrypt )
                {
                    string path = a.Encrypt;
                    Plan plan = YamlHelpers.DeserializeFile<Plan>( path );
                    plan = plan.EncryptElements();
                    if( a.HasOut )
                        path = a.Out;
                    YamlHelpers.SerializeFile( path, plan );
                }
                else
                {
                    string path = a.Decrypt;
                    Plan plan = YamlHelpers.DeserializeFile<Plan>( path );
                    plan = plan.DecryptElements();
                    if( a.HasOut )
                        path = a.Out;
                    YamlHelpers.SerializeFile( path, plan );
                }
            }
            else if( a.IsSample )
            {
                CreateSamplePlan( a.Sample, a.Out, a.Verbose );
            }
            else if( a.Render != RenderAction.None )
            {
                if( a.Render == RenderAction.Encode )
                    Console.WriteLine( CryptoHelpers.Encode( a.Plan ) );
                else
                    Console.WriteLine( a.Plan );
            }
            else
            {
                ExecuteResult result = null;

                Plan plan = YamlHelpers.Deserialize<Plan>( a.Plan );
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
                        Console.WriteLine( a.Plan );
                        _isSingleTaskModel = true;
                        result = plan.ExecuteHandlerProcess_SingleAction( plan.Actions[0], a.Args, a.Data, a.DryRun );
                        break;
                    }
                }
                exitCode = (int)result.Status;
                //exitCode = (int)plan.Result.Status;
                if( a.TaskModel != TaskModel.Single && !string.IsNullOrWhiteSpace( a.ResultPlan ) )
                {
                    bool isTrue = false;
                    isTrue = Boolean.TryParse( a.ResultPlan, out isTrue );
                    if( isTrue )
                    {
                        string extension = new FileInfo( a.PlanFilePath ).Extension;
                        string resultFile = a.PlanFilePath.Replace( extension, $".result{extension}" );
                        File.WriteAllText( resultFile, plan.ResultPlan.ToYaml() );
                    }
                    else
                    {
                        File.WriteAllText( a.ResultPlan, plan.ResultPlan.ToYaml() );
                    }
                }
            }

            //Console.WriteLine( $"exitCode:{exitCode}" );
            Environment.Exit( exitCode );
        }

        private static void plan_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            string msg = e.SerializeSimple( _isSingleTaskModel );
            if( _isSingleTaskModel )
                msg = CryptoHelpers.Encode( msg );
            Console.WriteLine( msg );
        }

        private static void plan_LogMessage(object sender, LogMessageEventArgs e)
        {
            string msg = e.SerializeSimple( _isSingleTaskModel );
            if( _isSingleTaskModel )
                msg = CryptoHelpers.Encode( msg );
            Console.WriteLine( msg );
        }


        #region Create Sample Plan
        static void CreateSamplePlan(string handlerCsvList, string outPath, bool verbose = false)
        {
            if( string.IsNullOrWhiteSpace( handlerCsvList ) )
                return;

            Plan p = new Plan()
            {
                Name = "SamplePlan",
                Description = "Example Config/Parameters",
                StartInfo = null
            };
            if( verbose )
                p = Plan.CreateSample();

            string[] handlers = handlerCsvList.Split( ',' );
            foreach( string handlerType in handlers )
            {
                ActionItem a = new ActionItem()
                {
                    Name = handlerType,
                    Description = $"Example Action for {handlerType}."
                };
                a.Handler = new HandlerInfo();
                a.Actions = null;

                IHandlerRuntime hr = null;
                try
                {
                    hr = AssemblyLoader.Load( handlerType, handlerType );
                }
                catch { }

                if( hr != null )
                {
                    a.Description = $"Resolved Handler from [{hr.RuntimeType}].";
                    a.Handler.Type = handlerType;
                    a.Handler.Config = new ParameterInfo();
                    a.Handler.Config.Values = hr.GetConfigInstance();
                    a.Parameters = new ParameterInfo();
                    a.Parameters.Values = hr.GetParametersInstance();
                }
                else
                {
                    a.Handler.Type = $"** Error - Could not load [{handlerType}].";
                }

                p.Actions.Add( a );
            }

            if( !string.IsNullOrWhiteSpace( outPath ) )
                File.WriteAllText( outPath, p.ToYaml() );
            else
                Console.WriteLine( p.ToYaml() );
        }
        #endregion


        #region ensure database exists
        static void EnsureDatabaseExists()
        {
            try
            {
                Synapse.Core.DataAccessLayer.SynapseDal.CreateDatabase();
            }
            catch( Exception ex )
            {
                ConsoleColor defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( ex.Message );
                if( ex.HResult == -2146233052 )
                    Console.WriteLine( "Ensure the x86/x64 Sqlite folders are included with the distribution." );
                Console.ForegroundColor = defaultColor;
                Environment.Exit( 99 );
            }
        }
        #endregion


        #region Help
        static void WriteHelpAndExit(string errorMessage = null)
        {
            bool haveError = !string.IsNullOrWhiteSpace( errorMessage );

            ConsoleColor defaultColor = Console.ForegroundColor;

            Console_WriteLine( $"synapse.cli.exe, Version: {typeof( Program ).Assembly.GetName().Version}\r\n", ConsoleColor.Green );
            Console.WriteLine( "Syntax:" );
            //Console_WriteLine( "  synapse.cli.exe /plan:{0}filePath{1}|{0}encodedPlanString{1}", ConsoleColor.Cyan, "{", "}" );
            //Console.WriteLine( "    [/resultPlan:{0}filePath{1}|true] [/dryRun:true|false]", "{", "}" );
            //Console.WriteLine( "    [/taskModel:inProc|external] [/render:encode|decode] [dynamic parameters]\r\n" );
            Console_WriteLine( " synapse.cli.exe plan:{0}filePath{1} [dryRun:true|false]", ConsoleColor.Cyan, "{", "}" );
            Console.WriteLine( "   [resultPlan:{0}filePath{1}|true] [dynamic parameters]", "{", "}" );
            Console_WriteLine( "\r\n  - Execute Plans.\r\n", ConsoleColor.Green, "" );
            Console_WriteLine( "  plan{0,-8} - filePath: Valid path to plan file.", ConsoleColor.Green, "" );
            //Console.WriteLine( "{0,-15}- [or] encodedPlanString: Inline base64 encoded plan string.", "" );
            Console.WriteLine( "  dryRun{0,-6} Specifies whether to execute the plan as a DryRun only.", "" );
            Console.WriteLine( "{0,-15}  Default is false.", "" );
            Console.WriteLine( "  resultPlan{0,-2} - filePath: Valid path to write ResultPlan output file.", "" );
            Console.WriteLine( "{0,-15}- [or]: 'true' will write to same path as /plan as *.result.*", "" );
            //Console.WriteLine( "  /taskModel{0,-3}Specifies whether to execute the plan on an internal", "" );
            //Console.WriteLine( "{0,-15}  thread or shell process.  Default is InProc.", "" );
            //Console.WriteLine( "  /render{0,-6}- encode: Returns the base64 encoded value of the", "" );
            //Console.WriteLine( "{0,-15}  specifed plan file.", "" );
            //Console.WriteLine( "{0,-15}- decode: Returns the base64 decoded value of the specified", "" );
            //Console.WriteLine( "{0,-15}  encodedPlanString.", "" );
            Console.WriteLine( "  dynamic{0,-6}Any remaining arg:value pairs will passed to the plan", "" );
            Console.WriteLine( "{0,-15}  as dynamic parms.\r\n", "" );
            Console_WriteLine( " synapse.cli.exe encrypt|decrypt:{0}filePath{1} [out:{0}filePath{1}]", ConsoleColor.Cyan, "{", "}" );
            Console_WriteLine( "\r\n  - Encrypt/decrypt Plan elements based on Plan/Action Crypto sections.\r\n", ConsoleColor.Green, "" );
            Console.WriteLine( "  encrypt{0,5} - filePath: Valid path to plan file to encrypt.", "" );
            Console.WriteLine( "  decrypt{0,5} - filePath: Valid path to plan file to decrypt.", "" );
            Console.WriteLine( "  out{0,9} - filePath: Optional output filePath.", "" );
            Console.WriteLine( "     {0,10}If [out] not specified, will encrypt/decrypt in-place.\r\n", "" );
            Console_WriteLine( " synapse.cli.exe sample:{0}handlerLib:handlerName,...{1} [out:{0}filePath{1}]", ConsoleColor.Cyan, "{", "}" );
            Console.WriteLine( "   [verbose:true|false]", "{", "}" );
            Console_WriteLine( "\r\n  - Create a sample Plan with the specified Handler(s).\r\n", ConsoleColor.Green, "" );
            Console.WriteLine( "  sample{0,5}  - A csv list of handlerLib:handlerName pairs.", "" );
            Console.WriteLine( "  out{0,9} - filePath: Optional output filePath.", "" );
            Console.WriteLine( "     {0,10}If [out] not specified, will output to screen.", "" );
            Console.WriteLine( "  verbose{0,5} - If true, adds example values for all Plan options.", "" );

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
        const string __resultplan = "resultplan";
        const string __encrypt = "encrypt";
        const string __decrypt = "decrypt";
        const string __out = "out";
        const string __sample = "sample";
        const string __verbose = "verbose";

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
                        PlanFilePath = Args[__plan];
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
                    //Message = "No plan specified.";
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

                #region ResultPlan
                if( Args.Keys.Contains( __resultplan ) )
                {
                    ResultPlan = Args[__resultplan];
                    Args.Remove( __resultplan );
                }
                else
                {
                    ResultPlan = string.Empty;
                }
                #endregion

                #region Encrypt
                if( Args.Keys.Contains( __encrypt ) )
                {
                    Encrypt = Args[__encrypt];
                    if( !File.Exists( Args[__encrypt] ) )
                        Message = "  * Unable to resolve Plan to encrypt.";
                    Args.Remove( __encrypt );
                }
                else
                {
                    Encrypt = string.Empty;
                }
                #endregion

                #region Decrypt
                if( Args.Keys.Contains( __decrypt ) )
                {
                    Decrypt = Args[__decrypt];
                    if( !File.Exists( Args[__decrypt] ) )
                        Message = "  * Unable to resolve Plan to decrypt.";
                    Args.Remove( __decrypt );
                }
                else
                {
                    Decrypt = string.Empty;
                }
                #endregion

                #region Out
                if( Args.Keys.Contains( __out ) )
                {
                    Out = Args[__out];
                    Args.Remove( __out );
                }
                else
                {
                    Out = string.Empty;
                }
                #endregion

                #region Sample
                if( Args.Keys.Contains( __sample ) )
                {
                    Sample = Args[__sample];
                    Args.Remove( __sample );
                }
                else
                {
                    Sample = string.Empty;
                }
                #endregion

                #region Verbose
                if( Args.Keys.Contains( __verbose ) )
                {
                    bool verbose = false;
                    if( bool.TryParse( Args[__verbose], out verbose ) )
                        Verbose = verbose;
                    Args.Remove( __verbose );
                }
                else
                {
                    Verbose = false;
                }
                #endregion
            }

            IsParsed &= string.IsNullOrWhiteSpace( Message );
        }

        public Dictionary<string, string> Args { get; internal set; }
        public string Plan { get; set; }
        public string PlanFilePath { get; set; }
        public string Data { get; set; }
        public bool DryRun { get; set; }
        public TaskModel TaskModel { get; set; }
        public RenderAction Render { get; set; }
        public string ResultPlan { get; set; }
        public string Message { get; internal set; }
        public bool IsParsed { get; internal set; }
        public string Encrypt { get; internal set; }
        public string Decrypt { get; internal set; }
        public string Out { get; internal set; }
        public bool IsCrypto { get { return IsEncrypt || IsDecrypt; } }
        public bool IsEncrypt { get { return !string.IsNullOrWhiteSpace( Encrypt ); } }
        public bool IsDecrypt { get { return !string.IsNullOrWhiteSpace( Decrypt ); } }
        public bool HasOut { get { return !string.IsNullOrWhiteSpace( Out ); } }
        public string Sample { get; internal set; }
        public bool IsSample { get { return !string.IsNullOrWhiteSpace( Sample ); } }
        public bool Verbose { get; internal set; }

        Dictionary<string, string> ParseCmdLine(string[] args)
        {
            IsParsed = true;
            Dictionary<string, string> options = new Dictionary<string, string>();

            string pattern = "(?<argname>.*?):(?<argvalue>.*)";
            //string pattern = @"(?<argname>/\w+):(?<argvalue>.*)";
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