using System;

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
        public bool IsEmpty { get { return this == ExecuteResult.Emtpy; } }

        public int PId { get; set; }
        public StatusType Status { get; set; }
        public string ExitData { get; set; }
        public StatusType BranchStatus { get; set; }

        public void SetStatusChecked(StatusType status)
        {
            if( status > Status )
                Status = status;
        }

        public void SetBranchStatusChecked(StatusType status)
        {
            if( status > BranchStatus )
                BranchStatus = status;
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