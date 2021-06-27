using Coveo.Dal;

namespace fe
{
    [Table("IndexService.BackupEntry")]
    public class BackupEntry_Index : BackupEntry
    {
        public string LogicalIndex; // todo - add edge ?
    }
}