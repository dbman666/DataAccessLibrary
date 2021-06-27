using Coveo.Dal;

namespace fe
{
    [Table("IndexService.ElasticsearchIndexConfiguration")]
    public class ElasticsearchIndexConfiguration
    {
        [Host] public string Host;
        [IndexId] [Pk(true)] public string Id;
        public string IndexName;
        [Password] public string Password;
        [Port] public int? Port;
        public bool? Ssltransport;
        public string Username;
        public string UrlPrefix;
        public string AccessKey;
        public string SecretAccessKey;
        public string ClusterId;
        public string RoleARN;

        [Edge_Index_ElasticConfig(FkFieldName = "Id")] public Index Index;
    }
}