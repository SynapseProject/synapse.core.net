using System;

namespace Synapse.Core
{
    public class ExecuteResult: ICloneable<ExecuteResult>
    {
        public ExecuteResult()
        {
            Status = StatusType.None;
        }

        public static readonly ExecuteResult Emtpy = new ExecuteResult();
        public bool IsEmpty { get { return this == ExecuteResult.Emtpy; } }

        public int PId { get; set; }
        public StatusType Status { get; set; }
        public object ExitData { get; set; }

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