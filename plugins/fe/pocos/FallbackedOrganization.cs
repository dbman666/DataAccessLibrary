using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.FallbackedOrganization")]
    public class FallbackedOrganization : Versioned
    {
        [Pk] public string Id;
        public string PreviousType;
    }
}