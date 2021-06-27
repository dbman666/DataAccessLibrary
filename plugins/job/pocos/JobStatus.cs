using System;
using fe;
using Coveo.Dal;

namespace job
{
    [Table("job_svc.JobStatus")]
    public class JobStatus
    {
        [Json] public RefreshStatus Details;
        public DateTime? Expiration;
        public string Handler; // todo-poco - actually ec2id + dockerid in parenthesis

        [JobId] [Pk] public string JobId; // What I do is take JobStatus as the 'owner' of the jobid, since it lives there for a while before expiring.

        // And a living Job is 1-1 with its JobStatus. This allows Task (source and Seccache) to refer to the JobId and not dangle.
        public JobStatusEnum? Status;
        public string InstanceId; // I keep these 2 as strings, since the instance/org may have been deleted; alternative is to null them if status is 'done' ? also add edges if I want
        [OrgId] public string OrganizationId;

        [Edge_Job_JobStatus] public Job Job;
        [Edge_JobStatus_Task] public Task_Source Task;
    }
}