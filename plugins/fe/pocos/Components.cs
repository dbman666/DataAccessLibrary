using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("NodeManager.Components")]
    public partial class Components
    {
        [Pk] public string Id; // Component type ? add fk
        [Json] public ComponentDefinition Json;
        public int Version;
    }

    public class Parameter
    {
        public string _type;
        public string Name;
        public string DefaultValue;
        public string Tag;
    }

    public class ParametersDefinition
    {
        public string _type;
        public List<Parameter> Parameters;
        public List<string> Files;
    }

    public class InstanceDefinition
    {
        public string _type;
        public string TypeName;
        public string Description;
        public string PackageFileName;
        public string InstallCommand;
        public string UninstallCommand;
        public ParametersDefinition ParametersDefinition;
        public string ExecutablePath;
        public string CommandParameters;
        public bool IsNodeProcess;
    }

    public class ComponentDefinition
    {
        public string _type;
        public string Name;
        public string Version;
        public string Platform;
        public string Target;
        public string Description;
        public string Location;
        public string InstallCommand;
        public string UninstallCommand;
        public Row Environment;
        public ParametersDefinition ParametersDefinition;
        public List<InstanceDefinition> Instances;
    }
}