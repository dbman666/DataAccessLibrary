using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ExpiredOrganization")]
    public class ExpiredOrganization
    {
        public bool? BackedUp;
        [OrgId] [Pk(true)] public string Id;

        [Edge_Org_ExpiredOrg(FkFieldName = "Id")] public Organization Organization;
    }
}