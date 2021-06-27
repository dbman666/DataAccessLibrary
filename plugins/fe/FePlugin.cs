using System.Linq;
using Coveo.Dal;

namespace fe
{
    public class FePlugin : Plugin
    {
        public static FePlugin G = new FePlugin(Meta.G);
        
        private FePlugin(Meta p_Meta) : base(p_Meta, "fe", null, new[]
        {
            typeof(ClusterAgent),
            //typeof(ClusterField),
            typeof(ClusterInstance),
            typeof(ClusterMessagingResource),
            typeof(CrawlerInfo),
            //typeof(CrawlingModuleVersions),
            typeof(DefaultExtensionSettings),
            typeof(DefaultSubscription),
            typeof(ElasticsearchIndexConfiguration),
            typeof(ExpiredOrganization),
            typeof(Extension),
            typeof(GlobalExtension),
            typeof(IdleOrganization),
            typeof(Index),
            typeof(InstanceToTerminate),
            typeof(LicenseTemplate),
            typeof(License),
            //typeof(LimitHistory),
            typeof(LogicalIndex),
            //typeof(Mapping),
            typeof(OrganizationCluster),
            typeof(OrganizationTemplate),
            typeof(Organization),
            typeof(ProvisioningParameters),
            typeof(ProvisioningTask),
            //typeof(SalesforcePreset),
            typeof(Schedule_SecurityCache),
            typeof(Schedule_Source),
            typeof(SecurityCacheRefreshState),
            typeof(SecurityCacheConfiguration),
            typeof(SecurityCacheEntry),
            //typeof(SecurityProviderReference),
            typeof(SecurityProvider),
            typeof(SourceExtension),
            typeof(SourceState),
            typeof(Source),
            typeof(Subscription),
            typeof(Task_Index),
            typeof(Task_Platform),
            typeof(Task_SecurityCache),
            typeof(Task_Source),
            //typeof(BackupEntry_Index),
            //typeof(BackupEntry_SecurityCache),
            //typeof(FallbackedOrganization),
            //typeof(LimitStatusUpdate),
            typeof(SwitchRabbitsTask),
        })
        {
        }

        public Task_Source FindTask_SourceByJobId(Repo p_Repo, string p_JobId)
        {
            return p_Repo.GetTable<Task_Source>().FirstOrDefault(t => t.JobId == p_JobId);
        }
    }
}