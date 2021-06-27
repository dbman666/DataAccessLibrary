using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.Task")]
    public class Task_Platform : Task
    {
        [Edge_Org_Task(FkFieldName = "FkOrgId")] public Organization OrgForTask;
    }
}