using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Synapse.Core.Utilities
{
    public static class PlanExtensions
    {
        /// <summary>
        /// Returns all the DynamicParameters for a Plan.
        /// </summary>
        /// <param name="plan">The Plan to interrogate.</param>
        /// <param name="simplify">Optionally reduces the return set to client-related values (Description/Source).</param>
        /// <returns></returns>
        public static List<DynamicValue> GetDynamicValues(this Plan plan, bool simplify = true)
        {
            List<DynamicValue> result = new List<DynamicValue>();

            Stack<ConcurrentBag<ActionItem>> actions = new Stack<ConcurrentBag<ActionItem>>();
            actions.Push( plan.ActionsBag );

            while( actions.Count > 0 )
            {
                ConcurrentBag<ActionItem> list = actions.Pop();

                foreach( ActionItem a in list )
                {
                    if( a.Parameters?.Dynamic != null )
                        if( simplify )
                            foreach( DynamicValue dv in a.Parameters.Dynamic )
                                result.Add( dv.AsSimpleValue() );
                        else
                            result.AddRange( a.Parameters.Dynamic );

                    if( a.HasActions )
                        actions.Push( a.ActionsBag );
                }
            }

            return result;
        }

        public static ConcurrentBag<ActionItem> ToConcurrentBag(this List<ActionItem> actions)
        {
            ConcurrentBag<ActionItem> list = new ConcurrentBag<ActionItem>();
            foreach( ActionItem actionItem in actions )
                list.Add( actionItem );
            return list;
        }

        public static List<ActionItem> ToStandardList(this ConcurrentBag<ActionItem> actions)
        {
            return new List<ActionItem>( actions );
        }
    }
}