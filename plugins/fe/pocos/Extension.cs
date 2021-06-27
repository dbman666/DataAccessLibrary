using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("ExtensionService.Extension")]
    public class Extension : Versioned
    {
        public string Description;
        public bool? Enabled;
        public int? ExecutionTimeout;
        [ExtensionId] [Pk] public string Id;
        public string Name;
        [OrgId] public string OrganizationId;
        [Json] public string RequiredDataStreams; // List<ExtensionDataStream>, but 1- it takes a lots of space in the cells; 2- it always complains there's no pk (well maybe for this one, we could discover it's an enum and let go)
        public string VersionId;
        public Language Language;
        public string ApiVersion;

        [Edge_Org_Extension] public Organization Organization;
        [Edge_Extension_SourceExtension] public List<SourceExtension> SourceExtensions;
    }

    public enum ExtensionDataStream
    {
        BODY_TEXT,
        BODY_HTML,
        THUMBNAIL,
        DOCUMENT_DATA
    }

    public enum Language
    {
        PYTHON,
        PYTHON3
    }
}