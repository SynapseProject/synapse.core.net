using System;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public class ExecuteResult : ICloneable<ExecuteResult>
    {
        public ExecuteResult()
        {
            Status = StatusType.None;
            BranchStatus = StatusType.None;
        }

        public static readonly ExecuteResult Emtpy = new ExecuteResult();
        [YamlIgnore]
        public bool IsEmpty { get { return this == ExecuteResult.Emtpy; } }

        public int PId { get; set; }
        public StatusType Status { get; set; }
        public string ExitData { get; set; }
        public StatusType BranchStatus { get; set; }

        //public void SetStatusChecked(StatusType status)
        //{
        //    if( status > Status )
        //        Status = status;
        //}

        //public void SetBranchStatusChecked(StatusType localStatus, StatusType descendentStatus)
        //{
        //    if( localStatus > BranchStatus )
        //        BranchStatus = localStatus;
        //    if( descendentStatus > BranchStatus )
        //        BranchStatus = descendentStatus;
        //    if( Status > BranchStatus )
        //        BranchStatus = Status;
        //}

        public void SetBranchStatusChecked(ExecuteResult compareResult)
        {
            //get the highest Status value
            StatusType compareStatus =
                compareResult.Status > compareResult.BranchStatus ? compareResult.Status : compareResult.BranchStatus;
            if( Status > compareStatus )
                compareStatus = Status;

            //compare highest value to current BranchStatus
            if( compareStatus > BranchStatus )
                BranchStatus = compareStatus;
        }


        object ICloneable.Clone()
        {
            return Clone( true );
        }

        public ExecuteResult Clone(bool shallow = true)
        {
            return (ExecuteResult)MemberwiseClone();
        }

        public override string ToString()
        {
            return Status.ToString();
        }
    }
}