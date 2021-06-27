using Coveo.Dal;

namespace fe
{
    [Table("SourceService.SourceExtension")]
    public class SourceExtension
    {
        public SourceExtensionDocumentAction? ActionOnError;
        public string Condition;
        [ExtensionId] public string ExtensionId;
        [Pk] public string Id; // todo-poco - where does that come from ? it's an int, so I'm not pk-compatible
        public string ItemType;
        public int? Order;
        [Json] public string Parameters;
        [SourceId] public string SourceId;
        public SourceExtensionStage? Stage;
        public string VersionId;

        [Edge_Extension_SourceExtension] public Extension Extension;
        [Edge_Source_SourceExtension] public Source Source;
    }
}