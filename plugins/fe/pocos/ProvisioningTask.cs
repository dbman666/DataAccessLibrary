using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ProvisioningTask")]
    public class ProvisioningTask : Versioned
    {
        public bool? DeleteAfterRun;
        public string Exception;
        public bool? InitialProvisioning;
        public bool? IsDone;
        public DateTime? LastProvisioningCompletedDate;
        public DateTime? LastProvisioningStartDate;
        [Json] public List<TopoOngoingResource> OngoingResources;
        [OrgClusterId] [Pk(true)] public string OrganizationClusterId;
        [OrgId] public string OrganizationId;
        public bool? RetryScheduled;
        public bool? Running;

        [Edge_OrgCluster_ProvTask(FkFieldName = "OrganizationClusterId")] public OrganizationCluster Cluster;
    }

    public class TopoOngoingResource
    {
        public string name;
        public string type;
        public string actualId;
        public string activityId;
        public bool? activityPaused;
        public string method;
        public string finishedMethod;
        public DateTime? timeCreated;
        public DateTime? timeStarted;
        public string state;
        public string exception;
        public int? numberOfRetries;
        public bool? statusReporter;
        public bool? timedOut;
    }
}