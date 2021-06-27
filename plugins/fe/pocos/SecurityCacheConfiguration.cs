using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.SecurityCacheConfiguration")]
    public class SecurityCacheConfiguration : Versioned
    {
        [Json] public SecurityCacheConfig Config;
        [OrgId] [Pk(true)] public string OrganizationId;

        [Edge_Org_SecCache] public Organization Organization;
        [Edge_SecCache_Schedule] public List<Schedule_SecurityCache> Schedules;
        //[Edge_SecCache_Task] public List<Task_SecurityCache> Tasks;
    }

    public class SecurityCacheConfig
    {
        public bool? jobProcessingEnabled;
        public bool? syncProcessingEnabled;
        public int? permissionModelsUpdateDelayInMilliseconds;
        public int? commitDelayInSeconds;
        public int? statusUpdateFrequencyInSeconds;
        public int? operationExpirationDelayInSeconds;
        public bool? metricsPublishingEnabled;
        public int? jobProcessorForwarderLRUCacheMaxSize;
    }
}