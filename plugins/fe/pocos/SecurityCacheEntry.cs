using System;
using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.SecurityCacheEntry")]
    public class SecurityCacheEntry : Versioned
    {
        [InstanceId] [Pk(true)] public string Id;
        public bool? Online;
        public string OrganizationId;
        public SecurityCacheStateState? State;
        public SecurityCacheType? Type;
        public DateTime? ProvisioningDate;

        [Edge_Org_SecCacheEntry] public Organization Organization;
        [Edge_Instance_SecCache] public ClusterInstance Instance;
        [Edge_SecCache_SecCacheStatus] public SecCacheStatus SecCacheStatus;
    }
}