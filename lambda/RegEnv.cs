namespace Coveo.Dal
{
    public class Reg
    {
        public string Abbr;
        public string Name;
        public Amazon.RegionEndpoint AwsRegion;

        public Reg(string abbr, string name)
        {
            Abbr = abbr;
            Name = name;
            AwsRegion = Amazon.RegionEndpoint.GetBySystemName(name);
        }

        private static Reg _this;
        public static Reg This => _this ??= Find(Var.Get("AWS_REGION")); 

        private static Reg Find(string name)
        {
            return name switch
            {
                "us-east-1" => new Reg("n", "us-east-1"),
                "eu-west-1" => new Reg("i", "eu-west-1"),
                "ap-southeast-2" => new Reg("a", "ap-southeast-2"),
                _ => null
            };
        }
    }

    public class Env
    {
        public string Abbr;
        public string Name;

        public Env(string abbr, string name)
        {
            Abbr = abbr;
            Name = name;
        }

        private static Env _this;
        public static Env This => _this ??= Find(Var.Get("coveo_env"));

        private static Env Find(string name)
        {
            return name switch
            {
                "dev" => new Env("d", "dev"),
                "qa" => new Env("q", "qa"),
                "prd" => new Env("p", "prd"),
                "hip" => new Env("h", "hip"),
                _ => null
            };
        }
    }

    public class RegEnv
    {
        public Reg Reg;
        public Env Env;
        public DbFeEndpoint DbFeEndpoint;
        public DbJobEndpoint DbJobEndpoint;
        public EsDoclogsEndpoint EsDoclogsEndpoint;
        public EsActEndpoint EsActEndpoint;

        private static RegEnv _this;
        public static RegEnv This => _this ??= Find(Reg.This, Env.This);

        public static RegEnv Find(Reg reg, Env env)
        {
            return (reg.Abbr + env.Abbr) switch
            {
                "nd" => new RegEnv(reg, env,
                    "ndev-platform57-0.cvk5xlkk0wai.us-east-1.rds.amazonaws.com",
                    "ndev-jobapi57-0.cvk5xlkk0wai.us-east-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-ndev-document-logs-v7-ogldgeonc4sce66asmlby67fru.us-east-1.es.amazonaws.com", "ndev-document-logs-v7"),
                    new EsActEndpoint("https://vpc-ndev-activity-mh3atoiyran4d6xs7glo6ycdre.us-east-1.es.amazonaws.com", "ndev-activity")),
                "nq" => new RegEnv(reg, env,
                    "nqa-platform57-cluster.cluster-cvk5xlkk0wai.us-east-1.rds.amazonaws.com",
                    "nqa-jobapi57-0.cvk5xlkk0wai.us-east-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-nqa-document-logs-v7-rjuziwutcpldfbhm5l65q7igse.us-east-1.es.amazonaws.com", "nqa-document-logs-v7"),
                    new EsActEndpoint("https://vpc-nqa-activity-4436dq4dlgun7rutxjrgnc4xha.us-east-1.es.amazonaws.com", "nqa-activity")),
                "np" => new RegEnv(reg, env,
                    "nprd-platform57-cluster.cluster-cupgqbweuwxq.us-east-1.rds.amazonaws.com",
                    "nprd-jobapi57-1.cupgqbweuwxq.us-east-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-nprd-document-logs-v7-anaqmagvsercwsponj5vajxy2i.us-east-1.es.amazonaws.com", "nprd-document-logs-v7"),
                    new EsActEndpoint("https://vpc-nprd-activity-jjxiowxrdlw2pv5lh3jqvhg7ze.us-east-1.es.amazonaws.com", "nprd-activity")),
                "nh" => new RegEnv(reg, env,
                    "nhip-platform57-cluster.cluster-cwsi0wy5x6bo.us-east-1.rds.amazonaws.com",
                    "nhip-jobapi57-cluster.cluster-cwsi0wy5x6bo.us-east-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-nhip-document-logs-v7-xc6qeskrzmp727qab6mnt6gtoe.us-east-1.es.amazonaws.com", "nhip-document-logs-v7"),
                    new EsActEndpoint("https://vpc-nhip-activity-uwepvfua6ckbccz5yic3zavihe.us-east-1.es.amazonaws.com", "nhip-activity")),
                "id" => new RegEnv(reg, env,
                    "idev-platform57-cluster.cluster-cu0vdzawgxcv.eu-west-1.rds.amazonaws.com",
                    "idev-jobapi57-cluster.cluster-cu0vdzawgxcv.eu-west-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-idev-document-logs-v7-rhhq72ocuoek4me6pj3q2n4pia.eu-west-1.es.amazonaws.com", "idev-document-logs-v7"),
                    new EsActEndpoint("https://vpc-idev-activity-e3x477ocvaxwys3x7vu6vncklu.eu-west-1.es.amazonaws.com", "idev-activity")),
                "iq" => new RegEnv(reg, env,
                    "iqa-platform57-cluster.cluster-cu0vdzawgxcv.eu-west-1.rds.amazonaws.com",
                    "iqa-jobapi57-cluster.cluster-cu0vdzawgxcv.eu-west-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-iqa-document-logs-v7-a2y3ba52n6vzcu5dj6zpzoxxim.eu-west-1.es.amazonaws.com", "iqa-document-logs-v7"),
                    new EsActEndpoint("https://vpc-iqa-activity-lndxwlo6rfue6hcpgr6fl6u3we.eu-west-1.es.amazonaws.com", "iqa-activity")),
                "ip" => new RegEnv(reg, env,
                    "iprd-platform57-cluster.cluster-chs0zssn3i0f.eu-west-1.rds.amazonaws.com",
                    "iprd-jobapi57-0.chs0zssn3i0f.eu-west-1.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-iprd-document-logs-v7-vryiyduyk4h6gexl2oy4o4744a.eu-west-1.es.amazonaws.com", "iprd-document-logs-v7"),
                    new EsActEndpoint("https://vpc-iprd-activity-rkkr6o3yc4oodx6kv5f33pptke.eu-west-1.es.amazonaws.com", "iprd-activity")),
                "ap" => new RegEnv(reg, env,
                    "aprd-platform57-1.csfctrvrswmu.ap-southeast-2.rds.amazonaws.com",
                    "aprd-jobapi57-1.csfctrvrswmu.ap-southeast-2.rds.amazonaws.com",
                    new EsDoclogsEndpoint("https://vpc-aprd-document-logs-v7-nt3zauuu2fn7eazzoetgq7woqy.ap-southeast-2.es.amazonaws.com", "aprd-document-logs-v7"),
                    new EsActEndpoint("https://vpc-aprd-activity-cbbgdwrwjkvylj5wbt5vhyzdpe.ap-southeast-2.es.amazonaws.com", "aprd-activity")),
                _ => null
            };
        }

        public RegEnv(Reg reg, Env env, string dbFeEndpoint, string dbJobEndpoint, EsDoclogsEndpoint esDoclogsEndpoint, EsActEndpoint esActEndpoint)
        {
            Reg = reg;
            Env = env;
            DbFeEndpoint = new DbFeEndpoint(this, dbFeEndpoint);
            DbJobEndpoint = new DbJobEndpoint(this, dbJobEndpoint);
            EsDoclogsEndpoint = esDoclogsEndpoint;
            EsActEndpoint = esActEndpoint;
        }
    }
}
