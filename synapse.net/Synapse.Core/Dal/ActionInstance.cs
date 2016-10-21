using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Synapse.Core.DataAccessLayer
{
    public struct ActionInstanceFields
    {
        public const string InstanceId = "Action_Instance_Id";
        public const string ActionName = "Action_Instance_Id";
        public const string ActionDetail = "Action_Instance_Id";
    }

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
            string sql = $@"insert into Action_Instance 
                {ActionInstanceFields.ActionName} = {action.Name}
                ,{ActionInstanceFields.ActionDetail} = {action.ToString()}
                ";
        }

        private void UpdateAction(ref ActionItem action)
        {
        }

        public void DeleteAction(int instanceId)
        {
        }
    }
}