using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("IndexService.LogicalIndex")]
    public class LogicalIndex : Versioned
    {
        public string Id;
        public string Name;
        [OrgId] public string OrganizationId;
        public DateTime? ProvisioningDate;

        [Pk][Computed] public string FullId;

        [Edge_Org_LogicalIndex] public Organization Organization;
        [Edge_Index_LogicalIndex(FkFieldName = "FullId")] public List<Index> Indices;
        
        public void PostLoad()
        {
            FullId = OrganizationId + Ctes.SEP_PK + Id;
        }
    }
}