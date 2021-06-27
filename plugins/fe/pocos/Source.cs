using System.Collections.Generic;
using System.Linq;
using Coveo.Dal;

namespace fe
{
    [Table("SourceService.Source", UseSelectStar = false)]
    public class Source : Versioned
    {
        public string AdditionalComments;
        [Json] public AdditionalInfo AdditionalInfo;
        public bool? ApplyChangesRequired;
        public int CesSourceId;
        public string CrawlerInstanceType;
        public bool? DedicatedJobHandlerEnabled;
        public InstanceHostType? HostType;
        [Pk][SourceId] public string Id;
        public string Name;
        [OrgId] public string OrganizationId;
        [Email] public string Owner;
        public SourcePriority? Priority;
        //public bool? SchedulesEnabled;
        [Json] [IsNotUseful] public string SourceRawConfig;
        public SourceType? SourceType;
        [IsNotUseful] public string CrawlerNodeId; // todo-poco - useless anymore; dunno when the column will be removed
        public string CrawlingModuleId;
        public string LogicalIndex; // todo - add edge ? It's mostly 'default' though; a bit overkill ?
        public bool StreamEnabled;

        [Edge_Org_Source] public Organization Organization;
        [Edge_Source_Mapping] public List<Mapping> Mappings;
        [Edge_Source_SourceExtension] public List<SourceExtension> SourceExtensions;
        [Edge_Source_SourceState] public SourceState SourceState;
        [Edge_Source_Task] public List<Task_Source> Tasks;
        [Edge_Source_Schedule] public List<Schedule_Source> Schedules;
        [Edge_Source_KnownMetaData] public List<KnownMetaData> KnownMetaDatas;
        [job.Edge_Job_Source] public List<job.Job> Jobs;
       
        // Extras
        [Computed] public SourceStats SourceStats;
        [Computed] public List<IndexSourceStatus> IndexSourceStatuses;

        public void PostEdges()
        {
            if (Organization?.Indices != null)
                foreach (var ix in Organization.Indices)
                    if (ix.IndexStatus?.Sources != null) {
                        if (IndexSourceStatuses == null)
                            IndexSourceStatuses = new List<IndexSourceStatus>();
                        IndexSourceStatuses.AddRange(ix.IndexStatus.Sources.Where(s => s.SourceId == CesSourceId).ToList());
                    }
        }
    }

    public class AdditionalInfo
    {
        public string salesforceUser;
        public string salesforceOrg;
        public string salesforceOrgName;
        public string schemaVersion;

        public bool? UseCommonClaimsProvider;
        public string InstanceId;
        public string tenantName;
        public bool? JiveInstanceAllowsAnonymousAccess;
        public string EmailAddress;
        public string teamName;
        public List<string> OCR_FILE_TYPES;
        public List<string> CustomParameterKeys;
        public string NewCrawlingModuleId;
        public string CrawlingAccount;
    }
}