namespace Coveo.Dal
{
    public class EsAwsEndpoint
    {
        public string Endpoint;
        public string Domain;
        public string Index;
        public string Alias;

        public EsAwsEndpoint(string endpoint, string domain, string index, string alias)
        {
            Endpoint = endpoint;
            Domain = domain;
            Index = index;
            Alias = alias;
        }
    }
    
    public class EsDoclogsEndpoint : EsAwsEndpoint
    {
        public EsDoclogsEndpoint(string endpoint, string domain) : base(endpoint, domain, "doclog*", "doclogs")
        {
        }
    }

    public class EsActEndpoint : EsAwsEndpoint
    {
        public EsActEndpoint(string endpoint, string domain) : base(endpoint, domain, "act*", null)
        {
        }
    }
}