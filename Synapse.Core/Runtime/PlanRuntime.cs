﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Synapse.Core
{
	//todo: multithread this with task.parallel, ensure thread safety on dicts
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

		Dictionary<string, Config> _configSets = new Dictionary<string, Config>();
		Dictionary<string, Parameters> _paramSets = new Dictionary<string, Parameters>();

		public HandlerResult Start(Dictionary<string, string> dynamicData, bool dryRun = false)
		{
			return ProcessRecursive( this.Actions, HandlerResult.Emtpy, dynamicData, dryRun );
		}

		public void Stop() { _wantsCancel = true; }

		public void Pause() { _wantsPause = true; }
		public void Continue() { _wantsPause = false; }

		HandlerResult ProcessRecursive(List<ActionItem> actions, HandlerResult result, Dictionary<string, string> dynamicData, bool dryRun = false)
		{
			HandlerResult returnResult = HandlerResult.Emtpy;
			IEnumerable<ActionItem> actionList = actions.Where( a => a.ExecuteCase == result.Status );

			foreach( ActionItem a in actionList )
			{
				string parms = ResolveConfigAndParameters( a, dynamicData );

				IHandlerRuntime rt = CreateHandlerRuntime( a.Name, a.Handler );
				rt.StepStarting += rt_StepStarting;
				rt.StepProgress += rt_StepProgress;
				rt.StepFinished += rt_StepFinished;

				HandlerResult r = rt.Execute( parms );

				if( r.Status > returnResult.Status ) { returnResult = r; }

				if( a.HasActions )
				{
					r = ProcessRecursive( a.Actions, r, dynamicData, dryRun );
					if( r.Status > returnResult.Status ) { returnResult = r; }
				}
			}

			return returnResult;
		}

		void rt_StepStarting(object sender, HandlerProgressCancelEventArgs e)
		{
			if( _wantsCancel ) { e.Cancel = true; }
			OnProgress( e );
		}

		void rt_StepProgress(object sender, HandlerProgressCancelEventArgs e)
		{
			OnProgress( e );
		}

		void rt_StepFinished(object sender, HandlerProgressCancelEventArgs e)
		{
			OnProgress( e );
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
				Config c = a.Handler.Config;
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
				Parameters p = a.Parameters;
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