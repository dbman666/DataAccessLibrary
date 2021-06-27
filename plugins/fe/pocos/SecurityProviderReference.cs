using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.SecurityProviderReference")]
    public class SecurityProviderReference
    {
        [Pk] public string Id;
        [OrgId] public string OrganizationId;
        public string ReferenceId;


        // todo-poco - so much trouble ! quadruple-check this one; it's embeddable; I think the id is generated; not even sure the table is good anymore
    }
}