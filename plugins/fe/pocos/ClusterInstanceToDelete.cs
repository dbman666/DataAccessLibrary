using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ClusterInstanceToDelete")]
    public class ClusterInstanceToDelete
    {
        [Pk] public string Id;
        public DateTime OfflineDate;
        public string OrganizationId;
        public string Type;

        [Edge_Org_ClusterInstancesToDelete] public Organization Organization;
    }
}