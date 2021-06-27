using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.CrawlingModuleVersions")]
    public class CrawlingModuleVersions : Versioned
    {
        [ComponentVersion] public string DatabaseVersion;
        [Pk] public string Id;
        [MaestroVersion] public string MaestroVersion;
        [OrgId] public string OrganizationId;
        public string Name;
        [ComponentVersion] public string WorkerVersion;
        [ComponentVersion] public string SecurityWorkerVersion;
        public bool Disabled;
        public DateTime LastVersionUpgrade;
        public DateTime LastHeartbeat;

        [Edge_Org_CrawlingModuleVersions] public Organization Organization;
    }
}