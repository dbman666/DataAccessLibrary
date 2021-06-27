using System;
using Coveo.Dal;

namespace fe
{
    [Table("FieldService.KnownMetaData")]
    public class KnownMetaData
    {
        [OrgId] public string OrganizationId;
        [SourceId] public string SourceId;
        public string Origin;
        public string Name;
        public DateTime LastReportedDate;
        public bool Ignored;
        
        [Pk][Computed] public string FullId;

        [Edge_Org_KnownMetaData] public Organization Organization;
        [Edge_Source_KnownMetaData] public Source Source;
        
        public void PostLoad()
        {
            FullId = OrganizationId + Ctes.SEP_PK + SourceId + Ctes.SEP_PK + Origin + Ctes.SEP_PK + Name;
        }
    }
}