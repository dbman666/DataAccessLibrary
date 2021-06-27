using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.Schedule")]
    public class Schedule_SecurityCache : Schedule
    {
        [Edge_SecCache_Schedule(FkFieldName = "FkSecCacheId")] public SecurityCacheConfiguration SecCache;
        [Edge_SecProv_Schedule(FkFieldName = "FkSecProvId")] public SecurityProvider SecProv;
    }
}