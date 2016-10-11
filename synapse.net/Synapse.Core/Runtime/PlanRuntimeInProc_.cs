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
    public partial class Plan
    {
        public HandlerResult Start_(Dictionary<string, string> dynamicData, bool dryRun = false, bool inProc = true)
        {
            if( inProc )
                return ProcessRecursive_( RunAs, null, Actions, HandlerResult.Emtpy, dynamicData, dryRun );
            else
                return ProcessRecursiveExternal( RunAs, Actions, HandlerResult.Emtpy, dynamicData, dryRun );
        }

        HandlerResult ProcessRecursive_(SecurityContext parentSecurityContext, ActionItem actionGroup, List<ActionItem> actions, HandlerResult result,
            Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            if( WantsStopOrPause() ) { return result; }

            HandlerResult returnResult = HandlerResult.Emtpy;

            StatusType queryStatus = result.Status;
            if( actionGroup != null && actionGroup.ExecuteCase == result.Status )
            {
                HandlerResult r = ExecuteHandlerProcess_( parentSecurityContext, actionGroup, dynamicData, dryRun );
                if( r.Status > returnResult.Status )
                    returnResult = r;

                if( actionGroup.HasActions )
                    r = ProcessRecursive_( parentSecurityContext, null, actionGroup.Actions, r, dynamicData, dryRun );
                if( r.Status > returnResult.Status )
                    returnResult = r;

                if( r.Status > queryStatus )
                    queryStatus = r.Status;
            }

            IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == queryStatus );
            Parallel.ForEach( actionList, a =>
                {
                    HandlerResult r = ExecuteHandlerProcess_( parentSecurityContext, a, dynamicData, dryRun );
                    if( a.HasActions )
                        r = ProcessRecursive_( a.RunAs, a.ActionGroup, a.Actions, r, dynamicData, dryRun );

                    if( r.Status > returnResult.Status )
                        returnResult = r;
                } );

            return returnResult;
        }

        HandlerResult ExecuteHandlerProcess_(SecurityContext parentSecurityContext, ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            HandlerResult returnResult = HandlerResult.Emtpy;

            string parms = ResolveConfigAndParameters( a, dynamicData );

            IHandlerRuntime rt = CreateHandlerRuntime( a.Name, a.Handler );
            rt.Progress += rt_Progress;

            if( !WantsStopOrPause() )
            {
                SecurityContext sc = a.HasRunAs ? a.RunAs : parentSecurityContext;
                sc?.Impersonate();
                HandlerResult r = rt.Execute( parms, dryRun );
                sc?.Undo();

                if( r.Status > returnResult.Status )
                    returnResult = r;
            }

            return returnResult;
        }
    }
}