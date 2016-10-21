using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Synapse.Core.DataAccessLayer
{
    public partial class SynapseDal
    {
        public Plan GetPlanById(int instanceId)
        {
            return null;
        }

        public int UpsertPlan(Plan plan)
        {
            if( plan.IsNew )
                CreatePlan( ref plan );
            else
                UpdatePlan( ref plan );

            return plan.InstanceId;
        }

        private void CreatePlan(ref Plan plan)
        {
        }

        private void UpdatePlan(ref Plan plan)
        {
        }

        public void DeletePlan(int instanceId)
        {
        }
    }
}