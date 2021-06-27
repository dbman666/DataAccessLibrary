using Coveo.Dal;

namespace fe
{
    [Table("FieldService.ClusterField")]
    public class ClusterField : Versioned
    {
        public string DateFormat;
        public string Description;
        public bool? Facet;
        public FieldType? FieldType;
        public bool? IncludeInQuery;
        public bool? IncludeInResults;
        public bool? MergeWithLexicon;
        public bool? MultiValueFacet;
        public string MultiValueFacetTokenizers;
        public string Name;
        [OrgId] public string OrganizationId;
        public bool? Ranking;
        public bool? SmartDateFacet;
        public bool? Sort;
        public FieldSourceType? SourceType;
        public bool? Stemming;
        public bool? UseCacheForComputedFacet;
        public bool? UseCacheForNestedQuery;
        public bool? UseCacheForNumericQuery;
        public bool? UseCacheForSort;
        public bool? HierarchicalFacet;

        [Pk][Computed] public string FullPk;

        [Edge_Org_ClusterField] public Organization Organization;

        public void PostLoad()
        {
            FullPk = OrganizationId + Ctes.SEP_PK + Name;
        }
    }
}