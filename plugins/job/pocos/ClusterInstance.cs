using System;
using Coveo.Dal;

namespace job
{
    [Table("job_svc.ClusterInstance")]
    public partial class ClusterInstance
    {
        public DateTime CreatedDate;
        public string DesiredStatus;
        public string InstanceId;
        public DateTime LastStateChangeDate;
        public DateTime UpdatedDate;
    }
}