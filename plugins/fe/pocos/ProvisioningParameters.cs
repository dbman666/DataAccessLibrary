using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ProvisioningParameters")]
    public class ProvisioningParameters : Versioned
    {
        [Json] public string AdditionalParameters;
        [OrgClusterId] [Pk(true)] public string Id;
        [Json] public string TrashedAgents;
        [Json] public string TrashedInstances;
        [Json] public string TrashedTopology;
        [Json] public string TrashedMessagingResources;

        [Edge_OrgCluster_ProvParameters(FkFieldName = "Id")] public OrganizationCluster Cluster;
    }
}