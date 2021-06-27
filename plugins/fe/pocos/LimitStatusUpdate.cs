using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.LimitStatusUpdate")]
    public class LimitStatusUpdate
    {
        [Pk] public string Id;
        public bool? InsertBlankStatuses;
        public string SectionName;
        public string Statuses;
        public bool? RemoveStatusesBeforeSaving;
    }
}