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

        public int PId { get; set; }
        public StatusType Status { get; set; }
        public int ExitCode { get; set; }
        public object ExitData { get; set; }
        public StatusType BranchStatus { get; set; }

        public int Sequence { get; set; }
        public string Message { get; set; }
        public string SecurityContext { get; set; }


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
            return $"{Status}/{BranchStatus}";
        }
    }
}