using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("IndexService.Index")]
    public class Index
    {
        [Json] [IsNotUseful] public string Config;
        [IndexId] [Pk] public string Id;
        public string Name;
        public bool Online;
        [OrgId] public string OrganizationId;
        public IndexType Type;
        public IndexState? State;
        public DateTime? ProvisioningDate;
        public string LogicalIndex;
        public Region Region;

        [Computed] public string FullLogicalIndexId;

        [Edge_Org_Index] public Organization Organization;
        [Edge_Index_ElasticConfig] public ElasticsearchIndexConfiguration ElasticConfig;
        [Edge_Index_Task] public List<Task_Index> Tasks;
        [Edge_Instance_Index] public ClusterInstance Instance;
        [Edge_Index_IndexStatus] public IndexStatus IndexStatus;
        [Edge_Index_LogicalIndex(FkFieldName = "FullLogicalIndexId")] public LogicalIndex LogicalIndex_;
        
        public void PostLoad()
        {
            FullLogicalIndexId = OrganizationId + Ctes.SEP_PK + LogicalIndex;
        }
    }
}