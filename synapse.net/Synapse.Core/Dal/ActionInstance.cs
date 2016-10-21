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
        public ActionItem GetActionById(int instanceId)
        {
            return null;
        }

        public int UpsertAction(ActionItem action)
        {
            if( action.IsNew )
                CreateAction( ref action );
            else
                UpdateAction( ref action );

            return action.InstanceId;
        }

        private void CreateAction(ref ActionItem action)
        {
        }

        private void UpdateAction(ref ActionItem action)
        {
        }

        public void DeleteAction(int instanceId)
        {
        }
    }
}