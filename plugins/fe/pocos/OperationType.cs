namespace fe
{
    public enum OperationType
    {
        REBUILD,
        FULL_REFRESH,
        INCREMENTAL_REFRESH,
        DELETION,
        SECURITY_CACHE_REFRESH, // Deprecated
        SECURITY_CACHE_REFRESH_ENTITIES_IN_ERROR, // Deprecated
        SECURITY_PROVIDER_REFRESH,
        SECURITY_PROVIDER_REFRESH_ENTITIES_IN_ERROR
    }
}
