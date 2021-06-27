using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ClusterMessagingResource")]
    public class ClusterMessagingResource
    {
        [Pk] public string Id;
        public string MessagingResourceId;
        [OrgClusterId] public string ClusterId;
        public string ServerId; // todo - should point on what's coming from Archaius
        public string Name;
        public TypeIdentifier DTYPE;
        public string Uri;
        
        [Edge_OrgCluster_MessagingResource] public OrganizationCluster Cluster;
    }
}