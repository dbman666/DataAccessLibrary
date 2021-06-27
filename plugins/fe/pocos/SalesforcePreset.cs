using Coveo.Dal;

namespace fe
{
    [Table("SourceService.SalesforcePreset")]
    public partial class SalesforcePreset
    {
        [Pk] public string Id;
        public string PresetConfig;
    }
}