using System;
using System.Collections.Generic;
using System.Linq;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.Organization")]
    public class Organization : Versioned
    {
        [OrgId] [Pk] public string Id;
        public string DisplayName;
        [Email] public string OwnerEmail;
        public bool? ReadOnly;
        public bool? SupportActivated;
        public bool? BeingDeleted;
        public bool? EmailNotificationsEnabled;
        public bool? PublicContentOnly;
        public string Type;
        public string PreviousType;
        public string[] CompositeLicenseTemplateIds;
        
        public List<ClusterAgent> Agents => Cluster?.Agents;
        public List<ClusterInstance> Instances => Cluster?.Instances;
        public List<ClusterMessagingResource> MessagingResources => Cluster?.MessagingResources;
        public ProvisioningParameters ProvParameters => Cluster?.ProvParameters;
        public ProvisioningTask ProvTask => Cluster?.ProvTask;

        [Edge_Org_OrgCluster] public OrganizationCluster Cluster;
        [Edge_Org_BackupEntry] public List<BackupEntry> Backups;
        [Edge_Org_ClusterField] public List<ClusterField> ClusterFields;
        [Edge_Org_ClusterInstancesToDelete] public List<ClusterInstanceToDelete> ClusterInstancesToDelete;
        [Edge_Org_CrawlingModuleVersions] public List<CrawlingModuleVersions> CrawlingModuleVersions;
        [Edge_Org_ExpiredOrg] public ExpiredOrganization ExpiredOrg;
        [Edge_Org_Extension] public List<Extension> Extensions;
        [Edge_Org_IdleOrg] public IdleOrganization IdleOrg;
        [Edge_Org_Index] public List<Index> Indices;
        [Edge_Org_LogicalIndex] public List<LogicalIndex> LogicalIndices;
        [Edge_Org_License] public License License;
        [Edge_Org_LimitStatus] public List<LimitStatus> LimitStatuses;
        [Edge_Org_LimitHistory] public List<LimitHistory> LimitHistories;
        [Edge_Org_Schedule] public List<Schedule> Schedules;
        [Edge_Org_SecCacheRefreshState] public List<SecurityCacheRefreshState> SecCacheRefreshStates;
        [Edge_Org_SecCache] public SecurityCacheConfiguration SecCache;
        [Edge_Org_SecCacheEntry] public List<SecurityCacheEntry> SecCacheEntries;
        [Edge_Org_SecProvider] public List<SecurityProvider> SecProviders;
        [Edge_Org_Source] public List<Source> Sources;
        [Edge_Org_Subscription] public List<Subscription> Subscriptions;
        [Edge_Org_AllTask] public List<Task> AllTasks;
        [Edge_Org_Task] public List<Task_Platform> Tasks;
        [Edge_Org_KnownMetaData] public List<KnownMetaData> KnownMetaDatas;
        [job.Edge_Org_Job] public List<job.Job> Jobs;

        [Computed][DontKeepType] private ContentLimits _contentLimits;
        [Computed][DontKeepType] public OrgStats OrgStats;
        
        public bool HasProvisioningDate => Cluster?.ProvTask?.LastProvisioningCompletedDate != null;
        public bool HasProvisionedAgents => Cluster?.Agents?.Where(a => a.InstanceId != null).Count() != 0;
        public bool IsExpired => DateTime.UtcNow > License.ExpirationDate;
        public bool IsPaused => IdleOrg?.Paused ?? false;
        public bool IsElastic => License?.IndexType == LicenseIndexType.ELASTIC;
        public bool IsIndexless => License?.IndexType == LicenseIndexType.INDEX_LESS;

        public static object FindOrElseOrDefaultOrThrow(Row p_Row1, string p_Field, Row p_Row2, long? p_Default)
        {
            var ret = p_Row1?[p_Field];
            if (ret == null)
                ret = p_Row2?[p_Field];
            if (ret == null)
                ret = p_Default;
            if (ret == null)
                throw new DalException($"Expected row to contain field {p_Field}.");
            return ret;
        }

        public ContentLimits ContentLimits(Repo p_Repo)
        {
            if (_contentLimits == null) {
                var content = (Row)License.PropertiesAsRow(p_Repo)["content"];
                var templateContent = (Row)License.Template?.PropertiesAsRow(p_Repo)?["content"];
                var searchapi = (Row)License.PropertiesAsRow(p_Repo)["searchapi"];
                var templateSearchapi = (Row)License.Template?.PropertiesAsRow(p_Repo)?["searchapi"];
                _contentLimits = new ContentLimits
                {
                    numberOfMappingsPerSourceLimit = (long)FindOrElseOrDefaultOrThrow(content, "numberOfMappingPerSourcesLimit", templateContent, DefaultLimits.NbMappingsPerSource),
                    numberOfFieldsLimit = (long)FindOrElseOrDefaultOrThrow(content, "numberOfFieldsLimit", templateContent, DefaultLimits.NbFieldsPerOrg),
                    numberOfSourcesLimit = (long)FindOrElseOrDefaultOrThrow(content, "numberOfSourcesLimit", templateContent, DefaultLimits.NbSourcesPerOrg),
                    numberOfSecurityProvidersLimit = (long)FindOrElseOrDefaultOrThrow(content, "numberOfSecurityProvidersLimit", templateContent, DefaultLimits.NbSecProvsPerOrg),
                    numberOfExtensionsPerSourceLimit = (long)FindOrElseOrDefaultOrThrow(content, "numberOfExtensionsPerSourceLimit", templateContent, DefaultLimits.NbExtensionsPerSource),
                    numberOfDailyPushApiCallsLimit = (long)FindOrElseOrDefaultOrThrow(content, "pushApiMaximumDailyInvocations", templateContent, DefaultLimits.NbDailyPushApiCallsPerOrg),
                    sizeOfDailyPushApiCallsLimit = (long)FindOrElseOrDefaultOrThrow(content, "pushApiFileSizeLimit", templateContent, DefaultLimits.SizeDailyPushApiCallsPerOrg),
                    numberOfDocumentsLimit = (long)FindOrElseOrDefaultOrThrow(content, "numberOfDocumentsLimit", templateContent, DefaultLimits.NbDocsPerOrg),
                    numberOfDailyQueriesLimit = (long)FindOrElseOrDefaultOrThrow(searchapi, "searchCountLimit", templateSearchapi, DefaultLimits.NbDailyQueriesPerOrg),
                };
            }
            return _contentLimits;
        }
    }
}