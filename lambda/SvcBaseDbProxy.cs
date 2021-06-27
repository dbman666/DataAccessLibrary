namespace Coveo.Dal
{
    public class SvcBaseDbProxy : SvcBase
    {
        public class ArgsBaseDbProxy : ArgsBase
        {
            public string Query;
        }

        public SvcBaseDbProxy(SvcLogger logger, SvcConfig config) : base(logger, config)
        {
        }

        public string Redirect(MysqlProxy dbClient, string query)
        {
            return dbClient.RawQueryToJson(null, query, "columns", "data").ToString();
        }
    }
}