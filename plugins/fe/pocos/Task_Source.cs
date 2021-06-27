using Coveo.Dal;

namespace fe
{
    [Table("SourceService.Task")]
    public class Task_Source : Task
    {
        public bool MonitoringPaused;

        [Edge_Source_Task(FkFieldName = "FkSourceId")] public Source Source;
        [Edge_Task_Activity] public act.Activity Activity;
    }
}