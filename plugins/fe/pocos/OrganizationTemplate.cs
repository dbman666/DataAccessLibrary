using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.OrganizationTemplate")]
    public class OrganizationTemplate : Versioned
    {
        public string ApiKeyTemplate;
        public IndexType? IndexType;
        [LicenseTemplateName] public string LicenseTemplateName;
        [OrgTemplateName] [Pk] public string Name;
        public string PrivilegeMaskId;
        public bool? PublicTemplate;
        public bool? PublicContentOnly;
        public bool? ShouldFallbackOnExpiration;

        [Edge_LicenseTemplate_OrgTemplate(FkFieldName = "LicenseTemplateName")] public LicenseTemplate Template;
    }
}