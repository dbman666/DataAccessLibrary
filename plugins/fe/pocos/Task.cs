using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table(null)]
    public class Task : Versioned
    {
        public IndexTaskDtype? DTYPE; // todo - poco one of the 4 sub-types
        [TaskId] [Pk] public string Id; // todo - poco one of the 4 sub-types
        [OrgId] public string OrganizationId;
        [ActivityId] public string ActivityId;
        [InstanceId] public string ResourceId; 
        [AgentId] public string AgentId;
        public TaskState? State;
        public IndexTaskType? Type;
        public bool? Global;
        public int? RetryCount;
        [TaskId] public string RelativeTaskId;
        public string CopyFromId;
        public bool? CompressData;
        public string BackupId;
        public string BackupLocation;
        public string RestoreFromId;
        public string SecurityProviderId;
        
        // Index
        public bool? SecCacheOperationStarted;
        public DateTime? RestartDate;
        [Json] public BaseCreateIndexModel CreateIndexModel;
        public int? SizeInGibibytes;
        public string Cluster;
        public string IndexName;
        public long? ActualDiskSpace;
        public string RabbitHost;
        public bool MainOperationCompleted;
        public string RequestId;
        public string LogicalIndex;
        public string Cause;
        public bool Cancelled;
        public string remediationState;
        public DateTime remediationStepStartDate;
        public bool PutOnline;
        
        // Platform
        public bool? RebuildSources;
        public string ConnectorsVersion;
        public string SecurityProvidersVersion;
        public string securityCachesVersion;
        public string indexesVersion;
        public bool? PausedOrganization;
        public string UpgradingInstances;
        public string UpgradingIndexes;
        public InstanceArchitecture? InstanceArchitecture;
        public string Info;
        public string InstancesToRevive;
        public string ComponentVersion;
        public bool CleanupInstancesEnabled;
        public string InstancesToShutdown;
        public string InstanceCopyMonitoringInfo;
        public string PendingInstances;
        public bool UpdateTags;

        // SecCache
        [OperationId] public string OperationId;
        public string Entity;
        public bool? RefreshStarted;
        [JobId] public string JobId;
        public bool? TriggeredBySchedule;
        public List<string> ScheduleIds;
        
        // Source
        public act.Operation? TargetActivityOperation;
        [TaskId] public string TaskIdToCancel;
        public bool? ApplyChangesWasRequired;
        public DateTime? StartedDate;
        public DateTime? LastUpdatedDate;
        public bool? CancelRequested;
        public string PreviouslyDisabledSchedules;
        public string AdditionalParameters;

        [Computed] public string FkIndexId;
        [Computed] public string FkOrgId;
        //[Computed] public string FkSecCacheId;
        [Computed] public string FkSecProvId;
        [Computed] public string FkSourceId;
        [Computed] public TimeSpan? Duration;

        [Edge_Org_AllTask] public Organization Organization;
        [job.Edge_JobStatus_Task] public job.JobStatus Job;

        public void PostLoad()
        {
            // bleh :(
            switch (DTYPE) {
            // Index
            case IndexTaskDtype.ElasticsearchIndexCreationTask:
            case IndexTaskDtype.ElasticsearchIndexDeletionTask:
            case IndexTaskDtype.IndexBackupTask:
            case IndexTaskDtype.IndexCreationTask:
            case IndexTaskDtype.IndexDeletionTask:
            case IndexTaskDtype.IndexResizeTask:
            case IndexTaskDtype.IndexRestoreTask:
            case IndexTaskDtype.IndexSynchronizationTask:
                FkIndexId = ResourceId;
                break;

            // Organization
            case IndexTaskDtype.OrganizationBackupTask:
            case IndexTaskDtype.OrganizationDeletionTask:
            case IndexTaskDtype.OrganizationImportTask:
            case IndexTaskDtype.OrganizationDuplicateTask:
            case IndexTaskDtype.OrganizationUpgradeTask:
            case IndexTaskDtype.OrganizationPausingTask:
            case IndexTaskDtype.OrganizationResumingTask:
            case IndexTaskDtype.AgentInstanceResizeTask:
                FkOrgId = OrganizationId;
                break;

            // SecurityCacheConfiguration
            case IndexTaskDtype.SecurityCacheRefreshTask:
            case IndexTaskDtype.SecurityCacheRefreshEntityTask:
            case IndexTaskDtype.SecurityCacheRefreshEntitiesInErrorTask:
            case IndexTaskDtype.SecurityCacheEnableDisabledEntitiesTask:
            case IndexTaskDtype.SecurityCacheBackupTask:
            case IndexTaskDtype.SecurityCacheRefreshEntitiesNotUpdatedTask:
            case IndexTaskDtype.SecurityCacheSynchronizationTask:
                //FkSecCacheId = OrganizationId; // yes, 1-1
                FkSecProvId = SecurityProvider.ComputedFullId(OrganizationId, SecurityProviderId);
                break;

            // SecurityProvider
            case IndexTaskDtype.ClusteredSecurityProviderCreationTask:
            case IndexTaskDtype.ClusteredSecurityProviderDeletionTask:
            case IndexTaskDtype.ClusteredSecurityProviderUpdateTask:
            case IndexTaskDtype.ClusteredSecurityProviderShutdownTask:
            case IndexTaskDtype.DedicatedSecurityProviderCreationTask:
            case IndexTaskDtype.DedicatedSecurityProviderDeletionTask:
            case IndexTaskDtype.DedicatedSecurityProviderUpdateTask:
            case IndexTaskDtype.DedicatedSecurityProviderShutdownTask:
            case IndexTaskDtype.NodelessSecurityProviderCreationTask:
            case IndexTaskDtype.NodelessSecurityProviderDeletionTask:
            case IndexTaskDtype.NodelessSecurityProviderUpdateTask:
            case IndexTaskDtype.NodelessSecurityProviderShutdownTask:
                FkSecProvId = SecurityProvider.ComputedFullId(OrganizationId, SecurityProviderId);
                break;

            // Source
            case IndexTaskDtype.CheckProvisioningTask:
            case IndexTaskDtype.CreateSourceTask:
            case IndexTaskDtype.UpdateSourceTask:
            case IndexTaskDtype.DeleteSourceTask:
            case IndexTaskDtype.RebuildSourceTask:
            case IndexTaskDtype.FullRefreshSourceTask:
            case IndexTaskDtype.IncrementalRefreshSourceTask:
            case IndexTaskDtype.CancelRefreshTask:
            case IndexTaskDtype.PauseSourceTask:
            case IndexTaskDtype.ResumeSourceTask:
            case IndexTaskDtype.ChangePushSourceStateTask:
                FkSourceId = ResourceId;
                break;
            
            default:
                throw new DalException("Unknown Task DTYPE.");
            }
            
            Duration = new TimeSpan(0, 0, (int)(DateTime.UtcNow - CreatedDate).TotalSeconds); // Chop off micros.
        }
    }
}