using System;
using act;
using Coveo.Dal;

namespace fe
{
    [Table("SourceService.SourceState")]
    public class SourceState
    {
        public int? CurrentRefreshNumberOfDocumentsProcessed;
        public string CurrentRefreshPausedOnErrorCode;
        public SourceRefreshType? CurrentRefreshType;
        public bool? InitialBuildCompleted;
        [ActivityId] public string LastRefreshActivityId;
        public DateTime? LastRefreshEndDate;
        public string LastRefreshErrorCode;
        public bool? LastRefreshInitialBuild;
        public int? LastRefreshNumberOfDocuments;
        public SourceRefreshResult? LastRefreshResult;
        public SourceRefreshType? LastRefreshType;
        public DateTime? LastStateChange;
        public DateTime? LastRebuildEndDate;
        public DateTime? LastFullRefreshEndDate;
        public DateTime? LastIncrementalRefreshEndDate;
        [SourceId] [Pk(true)] public string SourceId;
        public SourceStateType? Type;

        [Edge_Source_SourceState] public Source Source;
        [Edge_SourceState_Activity] public Activity LastRefreshActivity;
    }
}