using Coveo.Dal;

namespace fe
{
    [Table("SourceService.Schedule")]
    public class Schedule_Source : Schedule
    {
        [Edge_Source_Schedule(FkFieldName = "FkSourceId")] public Source Source;
    }
}