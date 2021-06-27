using System.Collections.Generic;
using System.Linq;
using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.SecurityProvider")]
    public class SecurityProvider : Versioned
    {
        public bool? CaseSensitive;
        [Json] [IsNotUseful] public string Config;
        public string DisplayName;
        public InstanceHostType? HostType;
        [InstanceId] public string Id;
        public string InstanceId;
        public string NodeTypeName; // See comment in SecurityProviderType. Some have a '.' in them.
        [OrgId] public string OrganizationId;
        public string SourceTypeName; // See comment in SecurityProviderType. Plus 'SharePoint Online' has a space in it :(
        public SecurityProviderState? State;
        public SecurityProviderType? Type;
        public bool? DedicatedJobHandlerEnabled;
        public string CrawlingModuleId;

        [Pk][Computed] public string FullId;

        [Edge_Org_SecProvider] public Organization Organization;
        [Edge_Provider_RefreshState] public List<SecurityCacheRefreshState> RefreshStates;
        [Edge_SecProv_Task] public List<Task_SecurityCache> Tasks;
        [Edge_SecProv_Schedule] public List<Schedule_SecurityCache> Schedules;
        [Edge_Instance_SecProv] public ClusterInstance Instance;
        [job.Edge_Job_Provider] public List<job.Job> Jobs;
        
        [Computed] public List<EntitiesCountPerStatePerProvider> EntitiesCountPerStatePerProvider;

        public void PostLoad()
        {
            FullId = ComputedFullId(OrganizationId, Id);
        }
        
        public void PostEdges()
        {
            if (Organization?.SecCacheEntries != null)
                foreach (var sc in Organization.SecCacheEntries)
                    if (sc.SecCacheStatus?.EntitiesCountPerStatePerProvider != null) {
                        if (EntitiesCountPerStatePerProvider == null)
                            EntitiesCountPerStatePerProvider = new List<EntitiesCountPerStatePerProvider>();
                        EntitiesCountPerStatePerProvider.AddRange(sc.SecCacheStatus.EntitiesCountPerStatePerProvider.Where(ec => ec.ProviderName == Id).ToList());
                    }
        }

        public static string ComputedFullId(string p_OrgId, string p_ProviderId)
        {
            return p_OrgId + Ctes.SEP_PK + p_ProviderId;
        }
    }
}