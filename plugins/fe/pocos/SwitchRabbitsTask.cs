using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.SwitchRabbitsTask")]
    public class SwitchRabbitsTask
    {
        [Pk][OrgId] public string OrganizationId;
        public string OrganizationClusterId;
        public string SourceRabbitId;
        public string TargetRabbitId;
        public DateTime StartedDate;
        public LicenseIndexType IndexType;
        public string RotationActivityId;
        public string QueuesToSwitch;
        public string ExchangesToSwitch;
        public string SourceIndexerIds;
        public string SourceSecurityCacheIds;
        public string SourceQueuesToJanitor;
    }
}