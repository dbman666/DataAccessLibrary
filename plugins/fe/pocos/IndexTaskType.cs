namespace fe
{
    public enum IndexTaskType
    {
        // todo-poco - these should be divided per sub-class
        
        // Index
        BACKUP,
        CREATION,
        DELETION,
        RESIZE,
        RESTORE,
        SYNCHRONIZATION,
        
        // Organization
        //BACKUP,
        //DELETION,
        DUPLICATE,
        IMPORT,
        UPGRADE,
        PAUSING,
        RESUMING,
        RESIZE_AGENT_INSTANCE,

        // SecurityCacheConfiguration
        REFRESH,
        REFRESH_ENTITY,
        REFRESH_ENTITIES_IN_ERROR,
        REFRESH_ENTITIES_NOT_UPDATED,
        ENABLE_DISABLED_ENTITIES,
        //BACKUP,
        //RESTORE,

        // SecurityProviders
        //CREATION,
        UPDATE,
        SHUTDOWN,
        //DELETION,
        
        // Source
        ORG_PROVISIONING_CHECK,
        CREATE,
        //UPDATE,
        DELETE,
        REBUILD,
        FULL_REFRESH,
        INCREMENTAL_REFRESH,
        REFRESH_CANCEL,
        PAUSE,
        RESUME,
        CHANGE_PUSH_SOURCE_STATE,
        RESUME_ALL
    }
}
