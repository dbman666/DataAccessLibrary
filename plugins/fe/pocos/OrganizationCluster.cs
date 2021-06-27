using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.OrganizationCluster")]
    public class OrganizationCluster : Versioned
    {
        [OrgClusterId] [Pk] public string Id;
        [Uri] public string BlobstoreUri;
        [Uri] public string DpmDocUri;
        [RabbitServerId] public string RabbitServerId;
        [OrgClusterId] [IsNotUseful] public string TopologyId;
        [Json] public ComponentVersions ComponentVersions;
        [ComponentVersion] public string PreviousComponentVersion; 
        public bool? LiveCluster;
        [OrgId] public string OrganizationId;
        public InstanceHostType? SecurityProvidersHostType;
        public InstanceHostType? ConnectorsHostType;
        [IsNotUseful] public string CrawlerDbEncryptedConnectionString;
        [IsNotUseful] public string SecurityProviderDbEncryptedConnectionString;
        public bool? OrphanedAgentDeletionAllowed;
        [rmq.ExchangeName] public string IndexDocQueueName;
        [rmq.QueueName] public string SecCacheJobQueueName;
        [rmq.ExchangeName] public string SecClusterSyncQueueName;
        [RabbitServerId] public string DpmRabbitServerId;
        [rmq.QueueName] public string DpmDocQueueName;
        public string SqsDpmDocQueueName;
        public string IdentityTargetMode; // BOTH, SECURITY_CACHE, EXPPP
        public string InstanceProvider;
        public string SecCacheJobQueueId;
        public string SecClusterSyncExchangeId;
        public string IndexDocDefaultExchangeId;
        public string DpmDefaultSqsQueueId;

        // Somehow these 2 are in euwest1
        [IsNotUseful] public string IndexRateUri;
        [IsNotUseful] public string IndexTagUri;

        [Edge_Org_OrgCluster] public Organization Organization;
        [Edge_OrgCluster_Agent] public List<ClusterAgent> Agents;
        [Edge_OrgCluster_Instance] public List<ClusterInstance> Instances;
        [Edge_OrgCluster_MessagingResource] public List<ClusterMessagingResource> MessagingResources;
        [Edge_OrgCluster_ProvParameters] public ProvisioningParameters ProvParameters;
        [Edge_OrgCluster_ProvTask] public ProvisioningTask ProvTask;
        
        // We could have a PostLoad here that would populate
    }

    public class ComponentVersions
    {
        public string id;
        public string connectors;
        public string indexer;
        public string securityCache;
        public string securityProvider;
    }
}