namespace fe
{
    public enum IndexTaskDtype
    {
        // todo-poco - these should be divided per sub-class
        
        // Index
        ElasticsearchIndexCreationTask,
        ElasticsearchIndexDeletionTask,
        IndexBackupTask,
        IndexCreationTask,
        IndexDeletionTask,
        IndexResizeTask,
        IndexRestoreTask,
        IndexSynchronizationTask,
        
        // Organization
        OrganizationBackupTask,
        OrganizationDeletionTask,
        OrganizationImportTask,
        OrganizationDuplicateTask,
        OrganizationUpgradeTask,
        OrganizationPausingTask,
        OrganizationResumingTask,
        AgentInstanceResizeTask,
        
        // SecurityCacheConfiguration
        SecurityCacheRefreshTask,
        SecurityCacheRefreshEntityTask,
        SecurityCacheRefreshEntitiesInErrorTask,
        SecurityCacheEnableDisabledEntitiesTask,
        SecurityCacheBackupTask,
        SecurityCacheRefreshEntitiesNotUpdatedTask,
        SecurityCacheSynchronizationTask,

        // SecurityProvider
        ClusteredSecurityProviderCreationTask,
        ClusteredSecurityProviderDeletionTask,
        ClusteredSecurityProviderUpdateTask,
        ClusteredSecurityProviderShutdownTask,
        DedicatedSecurityProviderCreationTask,
        DedicatedSecurityProviderDeletionTask,
        DedicatedSecurityProviderUpdateTask,
        DedicatedSecurityProviderShutdownTask,
        NodelessSecurityProviderCreationTask,
        NodelessSecurityProviderDeletionTask,
        NodelessSecurityProviderUpdateTask,
        NodelessSecurityProviderShutdownTask,
        
        // Source
        CheckProvisioningTask,
        CreateSourceTask,
        UpdateSourceTask,
        DeleteSourceTask,
        RebuildSourceTask,
        FullRefreshSourceTask,
        IncrementalRefreshSourceTask,
        CancelRefreshTask,
        PauseSourceTask,
        ResumeSourceTask,
        ChangePushSourceStateTask,
        ResumeAllSourcesTask
    }
}
