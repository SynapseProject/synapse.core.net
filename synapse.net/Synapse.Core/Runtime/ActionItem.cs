﻿using System;
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
    }
}