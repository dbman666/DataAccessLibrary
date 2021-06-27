using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.BackupEntry")]
    public class BackupEntry_Platform : BackupEntry
    {
        public long? Size;
    }
}