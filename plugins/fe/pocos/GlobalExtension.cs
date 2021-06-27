using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("ExtensionService.GlobalExtension")]
    public class GlobalExtension : Versioned
    {
        public string Description;
        public bool? Enabled;
        public int? ExecutionTimeout;
        [GlobalExtensionId] [Pk] public string Id;
        public string Name;
        [Json] public List<ExtensionDataStream> RequiredDataStreams;
        public string VersionId;
        public Language Language;
        public string ApiVersion;
    }
}