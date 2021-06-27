using Coveo.Dal;

namespace fe
{
    [Table("SecurityCacheService.Task")]
    public class Task_SecurityCache : Task
    {
        //[Edge_SecCache_Task(FkFieldName = "FkSecCacheId")] public SecurityCacheConfiguration SecCache;
        [Edge_SecProv_Task(FkFieldName = "FkSecProvId")] public SecurityProvider SecProv;
    }
}