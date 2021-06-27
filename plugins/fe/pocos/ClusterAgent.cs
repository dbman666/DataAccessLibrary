using System;
using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.ClusterAgent")]
    public class ClusterAgent
    {
        public InstanceArchitecture? Architecture;
        [InstanceAvailabilityZone] public string AvailabilityZone;
        [OrgClusterId] public string ClusterId;
        [Host] public string Host;
        [AgentId] [Pk(true)] public string Id;
        [Ec2Id] public string InstanceId;
        public bool? IsWindows;
        public DateTime? LaunchDate;
        [Port] public int? NextFreeAdminPort;
        [Port] public int? NextFreeSearchPort;
        [StringList] public string Roles;
        [Json] public List<ClusterAgentStorage> Storages;
        public Region Region;
        public int storageStripes;

        [Edge_NmAgent_ClusterAgentAttribute(FkFieldName = "Id")] public Agents NmAgent;
        [Edge_OrgCluster_Agent] public OrganizationCluster Cluster;
        [Edge_Agent_Instance] public List<ClusterInstance> Instances;
        
        public void PostLoad()
        {
            if (Storages == null)
                return;
            foreach (var storage in Storages) {
                storage.SizeInGibibytes = storage.StorageSpec.SizeInGibibytes;
                storage.StorageType = storage.StorageSpec.StorageType;
                storage.NumberOfIops = storage.StorageSpec.NumberOfIops;
                storage.StorageSpec = null;
            }
        }
    }

    public class ClusterAgentStorage
    {
        public StorageSpec StorageSpec;
        public int? SizeInGibibytes;
        public StorageType? StorageType;
        public int? NumberOfIops;
        public string DeviceName;
        public string VolumeId;
    }

    public class StorageSpec
    {
        public StorageType? StorageType;
        public int? SizeInGibibytes;
        public int? SizeInGigabytes; // Deprecated; bleh
        public int? NumberOfIops;
        public int? ThroughputInMebibytes;
    }
}