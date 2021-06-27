using System;
using fe;
using Coveo.Dal;

namespace job
{
    [Table("job_svc.Job", UseSelectStar = false)]
    public class Job : Versioned
    {
        public bool? Concurrent;
        public JobHandlerType? HandlerType;
        [JobId] [Pk(true)] public string Id;

        public string InstanceId;
        public string TaskClusterId;

        //[IsNotUseful] public string Job; // can't use field name 'Job' inside a class 'Job'. Plus it's quite heavy.
        public JobInterrupt? Interrupt;
        public JobType? JobType;
        [OrgId] public string OrganizationId;
        public int? Priority; // todo-poco - HIGHEST("Highest", 0), HIGH("High", 64), NORMAL("Normal", 128), LOW("Low", 192), LOWEST("Lowest", 255);
        public int? RemainingRetry;
        public JobStatusEnum? Status;
        public string Encrypter;
        [OperationId] public string OperationId;
        public DateTime? EffectiveDate;
        public string ComponentVersion;
        public string TaskContainerId;
        [TaskId] public string TaskId;
        public string CrawlingModuleId;
        public string OperationType;
        public DateTime? StartedDate;

        [Computed] public string FkSourceId;
        [Computed] public string FkProviderId;

        [Edge_Org_Job] public Organization Organization;
        [Edge_Job_Source(FkFieldName = "FkSourceId")] public Source Source;
        [Edge_Job_Provider(FkFieldName = "FkProviderId")] public SecurityProvider Provider;
        [Edge_Job_JobStatus] public JobStatus JobStatus;

        public void PostLoad()
        {
            if (JobType == job.JobType.Source && OperationId != "DELETE_SOURCE")
                FkSourceId = InstanceId;
            
            if (JobType == job.JobType.Provider)
                FkProviderId = SecurityProvider.ComputedFullId(OrganizationId, InstanceId);
        }
    }
}