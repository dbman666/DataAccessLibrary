namespace Coveo.Dal
{
    public class RegEnv
    {
        public string Region;
        public Amazon.RegionEndpoint AwsRegion => Amazon.RegionEndpoint.GetBySystemName(Region);
        public DbFeEndpoint DbFeEndpoint;
        public DbJobEndpoint DbJobEndpoint;
        public EsDoclogsEndpoint EsDoclogsEndpoint;
        public EsActEndpoint EsActEndpoint;

        private static RegEnv _this;

        public static RegEnv This => _this ??= new RegEnv
        {
            Region = Var.Get("AWS_REGION"),
            DbFeEndpoint = new DbFeEndpoint {Endpoint = Var.Get("db_fe_endpoint"), UsernameSsmKey = Var.Get("db_fe_username_ssm_key"), PasswordSsmKey = Var.Get("db_fe_password_ssm_key")},
            DbJobEndpoint = new DbJobEndpoint {Endpoint = Var.Get("db_job_endpoint"), UsernameSsmKey = Var.Get("db_job_username_ssm_key"), PasswordSsmKey = Var.Get("db_job_password_ssm_key")},
            EsDoclogsEndpoint = new EsDoclogsEndpoint(Var.Get("es_doclogs_endpoint"), Var.Get("es_doclogs_domain")),
            EsActEndpoint = new EsActEndpoint(Var.Get("es_activity_endpoint"), Var.Get("es_activity_domain"))
        };
    }
}
