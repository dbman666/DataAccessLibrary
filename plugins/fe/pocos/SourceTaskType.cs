namespace fe
{
    public enum SourceTaskType
    {
        ORG_PROVISIONING_CHECK,
        CREATE,
        UPDATE,
        DELETE,
        REBUILD,
        FULL_REFRESH,
        INCREMENTAL_REFRESH,
        REFRESH_CANCEL,
        PAUSE,
        RESUME,
        CHANGE_PUSH_SOURCE_STATE
    }
}
