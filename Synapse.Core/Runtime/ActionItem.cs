using System;
using System.Linq;
using System.Collections.Generic;

namespace Synapse.Core
{
    public partial class ActionItem
    {
        public void ResolveConfigAndParameters(Dictionary<string, string> dynamicData,
            Dictionary<string, ParameterInfo> globalConfigSets, Dictionary<string, ParameterInfo> globalParamSets)
        {
            List<ActionItem> resolvedActions = null;
            ResolveConfigAndParameters( dynamicData, globalConfigSets, globalParamSets, ref resolvedActions );
        }

        public void ResolveConfigAndParameters(Dictionary<string, string> dynamicData,
            Dictionary<string, ParameterInfo> globalConfigSets, Dictionary<string, ParameterInfo> globalParamSets, ref List<ActionItem> resolvedActions)
        {
            if( Handler == null )
                Handler = new HandlerInfo();

            List<object> forEachConfigs = new List<object>();
            if( Handler.HasConfig )
            {
                ParameterInfo c = Handler.Config;
                if( globalConfigSets != null && c.HasInheritFrom && globalConfigSets.Keys.Contains( c.InheritFrom ) )
                    c.InheritedValues = globalConfigSets[c.InheritFrom];

                c.Resolve( out forEachConfigs, dynamicData );

                if( globalConfigSets != null && c.HasName )
                    globalConfigSets[c.Name] = c;
            }
            else
            {
                Handler.Config = new ParameterInfo() { };
                forEachConfigs.Add( null );
            }

            List<object> forEachParms = new List<object>();
            if( HasParameters )
            {
                ParameterInfo p = Parameters;
                if( globalParamSets != null && p.HasInheritFrom && globalParamSets.Keys.Contains( p.InheritFrom ) )
                    p.InheritedValues = globalParamSets[p.InheritFrom];

                p.Resolve( out forEachParms, dynamicData );

                if( globalParamSets != null && p.HasName )
                    globalParamSets[p.Name] = p;
            }
            else
            {
                Parameters = new ParameterInfo() { };
                forEachParms.Add( null );
            }

            if( resolvedActions != null )
                foreach( object forEachConfig in forEachConfigs )
                    foreach( object forEachParm in forEachParms )
                    {
                        ActionItem clone = Clone( shallow: false );
                        clone.Handler.Config.Values = forEachConfig;
                        clone.Parameters.Values = forEachParm;

                        resolvedActions.Add( clone );
                    }
        }

        /// <summary>
        /// Evaluates RunAs and parentSecurity(parent RunAs) for whether to inherit settings.
        /// </summary>
        /// <param name="parentSecurityContext">Reference SecurityContext from parent Plan/Action.</param>
        public void IngestParentSecurityContext(SecurityContext parentSecurityContext)
        {
            if( HasRunAs )
                RunAs.InheritSettingsIfAllowed( parentSecurityContext );
            else if( parentSecurityContext != null && parentSecurityContext.IsInheritable )
                RunAs = parentSecurityContext;
        }
    }
}