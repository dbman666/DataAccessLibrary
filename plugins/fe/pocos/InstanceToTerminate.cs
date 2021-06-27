using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.InstanceToTerminate")]
    public class InstanceToTerminate
    {
        [Ec2Id] [Pk(true)] public string Id;
        public Region Region;
    }
}