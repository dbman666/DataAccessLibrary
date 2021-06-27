using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ClusterInstance")]
    public class ClusterInstance
    {
        public int? AdminPort;
        [Uri] public string AdminUri;
        [AgentId] public string AgentId;
        [OrgClusterId] public string ClusterId;
        public ComponentName? ComponentName;
        public ComponentPlatform? ComponentPlatform;
        [ComponentVersion] public string ComponentVersion;
        public InstanceDtype DTYPE;
        [Pk] public string Id;
        public string Name;
        
        // Index
        [rmq.QueueName] public string IndexerDocQueueName;
        public string IndexType; // pretty much deprecated
        [Port] public int? SearchPort;
        [Uri] public string SearchServerUri;
        public string IndexDocExchangeId;
        
        // SecCache
        [rmq.QueueName] public string SecCacheSyncQueueName;
        
        // SecProv
        [InstanceType] public string Type;
        public bool? CaseSensitive;
        public bool? UseDefaultConfiguration;
        
        [Computed] public string FkIndexId;
        [Computed] public string FkSecCacheId;
        [Computed] public string FkSecProvId;
        [Computed] public string FullComponentPk;

        [Edge_OrgCluster_Instance] public OrganizationCluster Cluster;
        [Edge_Agent_Instance] public ClusterAgent Agent;
        [Edge_Instance_Index(FkFieldName = "FkIndexId")] public Index Index;
        [Edge_Instance_SecCache(FkFieldName = "FkSecCacheId")] public SecurityCacheEntry SecCache;
        [Edge_Instance_SecProv(FkFieldName = "FkSecProvId")] public SecurityProvider SecProv;
        
        public void PostLoad()
        {
            switch (DTYPE) {
            case InstanceDtype.ClusterIndexer:
                FkIndexId = Id;
                break;
            case InstanceDtype.ClusterSecurityCache:
                FkSecCacheId = Id;
                break;
            case InstanceDtype.ClusterSecurityProvider:
                FkSecProvId = Id;
                break;
            }

            FullComponentPk = $"{ComponentName}|{ComponentVersion}|{ComponentPlatform}";
        }
    }
}