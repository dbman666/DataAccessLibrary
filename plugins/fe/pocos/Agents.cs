using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("NodeManager.Agents")]
    public partial class Agents
    {
        [Pk] [AgentId] public string Id;
        [Json] public NodeAgentDefinition Json;
        public int Version;

        [Edge_NmAgent_ClusterAgent] public ClusterAgent ClusterAgent;
    }

    public class NodeAgentConfig
    {
        public string _type;
        public string Name;
        public string Description;
        public string Uri;
        public string IP;
        public string ComponentsPath;
        public string InstancesPath;
        public int ReportingPeriod_s;
        public int ProcessTimeOut_s;
        public int NbRetriesOnFailure;
    }

    public class Component
    {
        public string _type;
        public ComponentDefinition Definition;
        public string Path;
        public Row DeploymentParams;
    }

    public class MonitoredProcessConfig
    {
        public string _type;
        public string ExecutablePath;
        public string CommandParameters;
        public string Path;
        public Row Environment;
        public bool IsStarted;
    }

    public class MonitoredProcess
    {
        public string Name;
        public string Description;
        public MonitoredProcessConfig Configuration;
    }

    public class InstanceFile
    {
        public string Id;
        public string Path;
        public string Hash;
    }

    public class NmInstance : MonitoredProcess
    {
        public string _type;
        public string ComponentName;
        public string ComponentVersion;
        public string InstanceType;
        public Row DeploymentParams;
        public List<InstanceFile> Files;
    }

    public class NodeAgentConnectionInfo
    {
        public string _type;
        public string NodeManagerUri;
        public string StatusTrackerUri;
        public string BlobStoreUri;
    }

    public class NodeAgentDefinition
    {
        public string _type;
        public string id;
        public NodeAgentConfig Configuration;
        public NodeAgentConnectionInfo ConnectionInfo;
        public List<Component> Components;
        public List<NmInstance> Instances;
        public List<MonitoredProcess> MonitoredProcesses;
        public string Platform;
        public string Version;
    }
}