using System;
using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.SecurityCacheRefreshState")]
    public class SecurityCacheRefreshState
    {
        public int? CurrentRefreshNumberOfEntitiesInError;
        public int CurrentRefreshNumberOfEntitiesProcessed;
        public DateTime? CurrentRefreshStartDate;
        public int? CurrentRefreshTotalNumberOfEntities;
        [Pk] public string Id;
        public DateTime? LastRefreshDate;
        public int LastRefreshNumberOfEntitiesProcessed;
        public RefreshResult? LastRefreshResult;
        public DateTime? LastRefreshStartDate;
        [OrgId] public string OrganizationId;
        public string ProviderId;
        public SecurityCacheTaskType? Type;

        [Computed] public string FkSecProvId;

        [Edge_Org_SecCacheRefreshState] public Organization Organization;
        [Edge_Provider_RefreshState(FkFieldName = "FkSecProvId")] public SecurityProvider Provider;

        public void PostLoad()
        {
            if (ProviderId != null)
                FkSecProvId = SecurityProvider.ComputedFullId(OrganizationId, ProviderId);
        }
    }
}