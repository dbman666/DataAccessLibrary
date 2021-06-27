using Coveo.Dal;

namespace fe
{
    [Table("IndexService.Task")]
    public class Task_Index : Task
    {
        [Edge_Index_Task(FkFieldName = "FkIndexId")] public Index Index;
    }

    public class BaseCreateIndexModel
    {
        public bool? online;

        // CreateCoveoIndexModel
        public string copyFromId;
        public MachineSpec machineSpec;
        public IndexingPipelineVersionsModel versions;
        public string copyFromAgent;
        public string indexId;

        // CreateElasticsearchIndexModel
        public string host;
        public int? port;
        public bool? ssl;
        public string urlPrefix;
        public string index;
        public SecurityConfiguration security;
    }

    public class IndexingPipelineVersionsModel
    {
        public string indexerVersion;
        public string securityCacheVersion;
    }

    public class SecurityConfiguration
    {
        // AWSAccessKeySecurityConfiguration
        public string accessKey;
        public string secretAccessKey;

        // AWSIAMRoleSecurityConfiguration
        public string roleARN;

        // BasicAuthSecurityConfiguration
        public string username;
        public string password;
    }

    public class MachineSpec
    {
        public InstanceArchitecture? architecture;
        public StorageSpec storageSpec;
    }
}