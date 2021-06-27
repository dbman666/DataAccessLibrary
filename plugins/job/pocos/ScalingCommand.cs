using System;
using Coveo.Dal;

namespace job
{
    [Table("job_svc.ScalingCommand")]
    public partial class ScalingCommand
    {
        public string ClusterId;
        public DateTime ExecutorUpdatedDate;
        public int InstanceDelta;
        public DateTime? PlannerUpdatedDate;
        public ScaleOperationStatus ScaleOperationStatus;
    }
}