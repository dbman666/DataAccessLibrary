using Coveo.Dal;

namespace fe
{
    [Table("SourceService.CrawlerInfo")]
    public class CrawlerInfo
    {
        public CrawlerCapabilities? Capabilities;
        [Pk] public string Id;
    }
}