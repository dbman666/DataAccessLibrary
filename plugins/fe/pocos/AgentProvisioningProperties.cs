using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.AgentProvisioningProperties")]
    public partial class AgentProvisioningProperties
    {
        public DateTime CreatedDate;
        public string InstanceProfile;
        public string KeyPairName;
        public string LinuxAMIId;
        public Region Region;
        public string SecurityGroupIds; // json: string[]
        public string SubnetPerAvailabilityZone; // json: map string string 
        public DateTime? UpdatedDate;
        public string Version;
        public string WindowsAMIId;
        public string EnvironmentVariables;
        public string NodeAgentContainerImageUri;

    }
}