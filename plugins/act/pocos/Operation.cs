namespace act
{
    public enum Operation
    {
        COMMIT,
        CREATE,
        DELETE,
        DISABLE,
        DUPLICATE,
        ENABLE,
        ENABLE_DISABLED_ENTITIES,
        EXPORT,
        UPDATE,
        START,
        STOP,
        PAUSE,
        PAUSE_ON_ERROR,
        RESUME,
        SYNCHRONIZE,
        BACKUP,
        REFRESH,
        REFRESH_ENTITIES_IN_ERROR,
        REFRESH_ENTITIES_NOT_UPDATED,
        REFRESH_ENTITY,
        RESIZE,
        RESTORE,
        ROTATE,
        IMPORT,
        CONFIG_CHANGE,
        CONFIG_CREATE,
        CHANGE_READ_ONLY,
        CHANGE_ONLINE,
        TEST,
        UPGRADE,
        SCHEDULE_CREATE,
        SCHEDULE_CHANGE,
        SCHEDULE_DELETE,
        ORG_PROVISIONING_CHECK,
        REBUILD,
        FULL_REFRESH,
        INCREMENTAL_REFRESH,
        IDLE,
        REFRESH_CANCEL,
        LIMIT_REACHED
    }
}

//private static final Set<ActivityOperation> CONTENT_EVENT_OPERATIONS = Sets.immutableEnumSet(INCREMENTAL_REFRESH,
//                                                                                                 FULL_REFRESH,
//                                                                                                 REBUILD);
//
//public static final Set<ActivityOperation> SOURCE_OPERATIONS = Sets.immutableEnumSet(REBUILD,
//                                                                                     FULL_REFRESH,
//                                                                                     INCREMENTAL_REFRESH,
//                                                                                     REFRESH_CANCEL,
//                                                                                     DELETE,
//                                                                                     PAUSE,
//                                                                                     RESUME)
