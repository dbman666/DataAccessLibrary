using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.IdleOrganization")]
    public class IdleOrganization : Versioned
    {
        [OrgId] [Pk(true)] public string Id;
        [StringList] public string IgnoredSecurityCacheScheduleIds;
        [StringList] public string IgnoredSourceScheduleIds;
        [StringList] public string IgnoredSecurityProviderScheduleIds;
        public bool? Paused;
        public bool? ExternalRequest;
        public PauseState PauseState;
        public DateTime? LastPausedDate;
        public DateTime? LastResumedDate;

        [Edge_Org_IdleOrg(FkFieldName = "Id")] public Organization Organization;
    }
}