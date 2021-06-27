namespace fe
{
    public enum SecurityCacheTaskDtype
    {
        // SecurityCacheConfiguration
        SecurityCacheRefreshTask,
        SecurityCacheRefreshEntityTask,
        SecurityCacheRefreshEntitiesInErrorTask,
        SecurityCacheEnableDisabledEntitiesTask,
        SecurityCacheBackupTask,

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
    }
}
