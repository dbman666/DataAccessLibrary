namespace fe
{
    public enum SecurityCacheTaskType
    {
        // SecurityCacheConfiguration
        REFRESH,
        REFRESH_ENTITY,
        REFRESH_ENTITIES_IN_ERROR,
        ENABLE_DISABLED_ENTITIES,
        BACKUP,
        RESTORE,

        // SecurityProviders
        CREATION,
        UPDATE,
        SHUTDOWN,
        DELETION
    }
}
