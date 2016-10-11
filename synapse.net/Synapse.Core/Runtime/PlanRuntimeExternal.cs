using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core.Utilities;

namespace Synapse.Core
{
    //todo: multithread this with task.parallel, ensure thread safety on dicts
    public partial class Plan
    {
        public HandlerResult StartExternal(Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            return ProcessRecursiveExternal( Actions, HandlerResult.Emtpy, dynamicData, dryRun );
        }

        HandlerResult ProcessRecursiveExternal(List<ActionItem> actions, HandlerResult result,
            Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            if( WantsStopOrPause() ) { return result; }

            HandlerResult returnResult = HandlerResult.Emtpy;
            IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == result.Status );

            Parallel.ForEach( actionList, actionItem =>
                {
                    HandlerResult r = ExecuteHandlerProcessExternal( actionItem, dynamicData, dryRun );
                    if( r.Status > returnResult.Status )
                        returnResult = r;
                } );

            return returnResult;
        }

        HandlerResult ExecuteHandlerProcessExternal(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            HandlerResult returnResult = HandlerResult.Emtpy;

            if( !WantsStopOrPause() )
            {
                HandlerResult r = SpawnExternal( a, dynamicData, dryRun );

                if( r.Status > returnResult.Status ) { returnResult = r; }

                if( a.HasActions )
                {
                    r = ProcessRecursiveExternal( a.Actions, r, dynamicData, dryRun );
                    if( r.Status > returnResult.Status )
                        returnResult = r;
                }
            }

            return returnResult;
        }

        private HandlerResult SpawnExternal(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun)
        {
            List<string> args = new List<string>();

            Plan container = new Plan();
            container.Name = $"{this.Name}:{a.Name}";
            container.Actions.Add( a );

            args.Add( $"/dryRun:{dryRun}" );
            args.Add( $"/thread:single" );
            foreach( string key in dynamicData.Keys )
                args.Add( $"/{key}:{dynamicData[key]}" );

            OnProgress( $" --> external --> {container.Name}", "external", string.Join( " ", args ) );
            Console.WriteLine( $" --> external --> {container.Name}" );

            string planYaml = CrytoHelpers.Encode( container.ToYaml() );
            args.Add( $"/plan:{planYaml}" );
            string arguments = string.Join( " ", args );

            Process p = new Process();
            p.StartInfo.Arguments = arguments;
            p.StartInfo.FileName = "synapse.cli.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.EnableRaisingEvents = true;
            //if( a.HasRunAs )
            //{
            //    p.StartInfo.Domain = a.RunAs.Domain;
            //    p.StartInfo.UserName = a.RunAs.UserName;
            //    SecureString pw = new SecureString();
            //    foreach( char c in a.RunAs.Password )
            //        pw.AppendChar( c );
            //    p.StartInfo.Password = pw;
            //}

            p.OutputDataReceived += p_OutputDataReceived;

            p.Start();

            #region read this
            // best practice information on accessing stdout/stderr from mdsn article:
            //  https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.redirectstandardoutput%28v=vs.110%29.aspx
            // Do not wait for the child process to exit before reading to the end of its redirected stream.
            // Do not perform a synchronous read to the end of both redirected streams.
            // string output = p.StandardOutput.ReadToEnd();
            // string error = p.StandardError.ReadToEnd();
            // p.WaitForExit();
            // Use asynchronous read operations on at least one of the streams.
            #endregion
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();


            p.WaitForExit();

            int exitCode = p.ExitCode;
            HandlerResult result = new HandlerResult();
            result.Status = (StatusType)exitCode;
            return result;
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                OnProgress( "a", "o", e.Data );
            }
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if( e.Data != null )
            {
                OnProgress( "a", "e", e.Data );
            }
        }

        public HandlerResult ExecuteHandlerProcess_SingleAction(ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            HandlerResult returnResult = HandlerResult.Emtpy;

            string parms = ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a.Name, a.Handler );
            rt.Progress += rt_Progress;

            if( !WantsStopOrPause() )
            {
                a.RunAs?.Impersonate();
                HandlerResult r = rt.Execute( parms, dryRun );
                a.RunAs?.Undo();

                if( r.Status > returnResult.Status )
                    returnResult = r;
            }

            return returnResult;
        }
    }
}


//HandlerResult ProcessRecursive_(List<ActionItem> actions, HandlerResult result, Dictionary<string, string> dynamicData, bool dryRun = false)
//{
//    if( WantsStopOrPause() ) { return result; }

//    HandlerResult returnResult = HandlerResult.Emtpy;
//    IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == result.Status );

//    foreach( ActionItem a in actionList )
//    {
//        if( WantsStopOrPause() ) { break; }

//        string parms = ResolveConfigAndParameters( a, dynamicData );

//        IHandlerRuntime rt = CreateHandlerRuntime( a.Name, a.Handler );
//        rt.Progress += rt_Progress;

//        if( WantsStopOrPause() ) { break; }
//        HandlerResult r = rt.Execute( parms, dryRun );

//        if( r.Status > returnResult.Status ) { returnResult = r; }

//        if( a.HasActions )
//        {
//            r = ProcessRecursive( a.Actions, r, dynamicData, dryRun );
//            if( r.Status > returnResult.Status ) { returnResult = r; }
//        }
//    }

//    return returnResult;
//}

