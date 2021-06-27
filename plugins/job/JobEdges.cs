using Coveo.Dal;

namespace job
{
    public class Edge_Org_JobAttribute : EdgeAttribute { }
    public class Edge_Job_SourceAttribute : EdgeAttribute { }
    public class Edge_Job_ProviderAttribute : EdgeAttribute { }
    public class Edge_JobStatus_TaskAttribute : EdgeAttribute { }
    public class Edge_Job_JobStatusAttribute : EdgeAttribute { } // Not kept because a JobStatus will live for some time after the Job is deleted, so the fk won't match.
}