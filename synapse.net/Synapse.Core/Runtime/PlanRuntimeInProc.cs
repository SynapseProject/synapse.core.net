﻿using System;
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
        #region events
        public event EventHandler<HandlerProgressCancelEventArgs> Progress;

        /// <summary>
        /// Notify of step start. If return value is True, then cancel operation.
        /// </summary>
        /// <param name="context">The method name or workflow activty.</param>
        /// <param name="message">Descriptive message.</param>
        /// <param name="status">Overall Package status indicator.</param>
        /// <param name="id">Message Id.</param>
        /// <param name="severity">Message/error severity.</param>
        /// <param name="ex">Current exception (optional).</param>
        /// <returns>HandlerProgressCancelEventArgs.Cancel value.</returns>
        protected virtual bool OnProgress(string actionName, string context, string message, StatusType status = StatusType.Running, int id = 0, int severity = 0, bool cancel = false, Exception ex = null)
        {
            HandlerProgressCancelEventArgs e =
                new HandlerProgressCancelEventArgs( context, message, status, id, severity, cancel, ex ) { ActionName = actionName };
            if( _wantsCancel ) { e.Cancel = true; }
            OnProgress( e );

            return e.Cancel;
        }

        /// <summary>
        /// Notify of step start. If e.Cancel is True, then cancel operation.
        /// </summary>
        protected virtual void OnProgress(HandlerProgressCancelEventArgs e)
        {
            if( Progress != null )
            {
                Progress( this, e );
            }
        }
        #endregion

        bool _wantsCancel = false;
        bool _wantsPause = false;

        Dictionary<string, ParameterInfo> _configSets = new Dictionary<string, ParameterInfo>();
        Dictionary<string, ParameterInfo> _paramSets = new Dictionary<string, ParameterInfo>();

        #region control methods
        public void Stop() { _wantsCancel = true; }
        public void Pause() { _wantsPause = true; }
        public void Continue() { _wantsPause = false; }
        public bool IsPaused { get { return _wantsPause; } }
        void CheckPause() //todo: this is just a stub method
        {
            if( _wantsPause )
            {
                System.Threading.Thread.Sleep( 1000 );
            }
        }

        bool WantsStopOrPause()
        {
            if( _wantsCancel )
            {
                return true;
            }
            else
            {
                if( _wantsPause )
                {
                    System.Threading.Thread.Sleep( 1000 );
                }
                return false;
            }
        }
        #endregion

        public HandlerResult Start(Dictionary<string, string> dynamicData, bool dryRun = false, bool inProc = true)
        {
            if( inProc )
                return ProcessRecursive( RunAs, Actions, HandlerResult.Emtpy, dynamicData, dryRun );
            else
                return ProcessRecursiveExternal( RunAs, null, Actions, HandlerResult.Emtpy, dynamicData, dryRun );
        }

        HandlerResult ProcessRecursive(SecurityContext parentSecurityContext, List<ActionItem> actions, HandlerResult result,
            Dictionary<string, string> dynamicData, bool dryRun = false)
        {
            if( WantsStopOrPause() ) { return result; }

            HandlerResult returnResult = HandlerResult.Emtpy;
            IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == result.Status );

            Parallel.ForEach( actionList, actionItem =>
                {
                    HandlerResult r = ExecuteHandlerProcess( parentSecurityContext, actionItem, dynamicData, dryRun );
                    if( r.Status > returnResult.Status )
                        returnResult = r;
                } );

            return returnResult;
        }

        HandlerResult ExecuteHandlerProcess(SecurityContext parentSecurityContext, ActionItem a, Dictionary<string, string> dynamicData, bool dryRun = false)
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

                if( a.HasActions )
                {
                    r = ProcessRecursive( sc, a.Actions, r, dynamicData, dryRun );
                    if( r.Status > returnResult.Status ) { returnResult = r; }
                }
            }

            return returnResult;
        }

        void rt_Progress(object sender, HandlerProgressCancelEventArgs e)
        {
            if( _wantsCancel ) { e.Cancel = true; }
            OnProgress( e );
            if( e.Cancel ) { _wantsCancel = true; }
        }

        string ResolveConfigAndParameters(ActionItem a, Dictionary<string, string> dynamicData)
        {
            bool cancel = OnProgress( a.Name, "ResolveConfigAndParameters", "Start" );
            if( cancel )
            {
                _wantsCancel = true;
                return null;
            }

            if( a.Handler.HasConfig )
            {
                ParameterInfo c = a.Handler.Config;
                if( c.HasInheritFrom && _configSets.Keys.Contains( c.InheritFrom ) )
                {
                    c.InheritedValues = _configSets[c.InheritFrom];
                }
                c.Resolve( dynamicData );

                if( c.HasName )
                {
                    _configSets[c.Name] = c;
                }
            }

            string parms = null;
            if( a.HasParameters )
            {
                ParameterInfo p = a.Parameters;
                if( p.HasInheritFrom && _paramSets.Keys.Contains( p.InheritFrom ) )
                {
                    p.InheritedValues = _paramSets[p.InheritFrom];
                }
                parms = p.Resolve( dynamicData );

                if( p.HasName )
                {
                    _paramSets[p.Name] = p;
                }
            }

            return parms;
        }

        IHandlerRuntime CreateHandlerRuntime(string actionName, HandlerInfo info)
        {
            bool cancel = OnProgress( actionName, "CreateHandlerRuntime: " + info.Type, "Start" );
            if( cancel )
            {
                _wantsCancel = true;
                return new Runtime.EmptyHandler();
            }

            IHandlerRuntime hr = new Runtime.EmptyHandler();

            string[] typeInfo = info.Type.Split( ':' );
            AssemblyName an = new AssemblyName( typeInfo[0] );
            Assembly hrAsm = Assembly.Load( an );
            Type handlerRuntime = hrAsm.GetType( typeInfo[1], true );
            hr = Activator.CreateInstance( handlerRuntime ) as IHandlerRuntime;
            hr.ActionName = actionName;

            string config = info.HasConfig ? info.Config.ResolvedValuesSerialized : null;
            hr.Initialize( config );

            return hr;
        }
    }
}