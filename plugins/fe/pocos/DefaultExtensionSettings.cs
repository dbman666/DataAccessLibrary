using Coveo.Dal;

namespace fe
{
    [Table("SourceService.DefaultExtensionSettings")]
    public class DefaultExtensionSettings
    {
        [Json] public string ExtensionSettings;
        [Pk] public string Name;
    }
}