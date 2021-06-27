namespace Coveo.Dal
{
    public abstract class MysqlEndpoint
    {
        public RegEnv RegEnv;
        public string Endpoint;
        public abstract string Username { get; }
        public abstract string Password { get; }

        public MysqlEndpoint(RegEnv regenv, string endpoint)
        {
            RegEnv = regenv;
            Endpoint = endpoint;
        }

        public MysqlProxy Client => new MysqlProxy(Endpoint, Username, Password);
    }

    public class DbFeEndpoint : MysqlEndpoint
    {
        public DbFeEndpoint(RegEnv regenv, string endpoint) : base(regenv, endpoint)
        {
        }

        public override string Username => Ssm.Get($"/{RegEnv.Env.Name}/CloudPlatform/Database/platform57/Username", false);
        public override string Password => Ssm.Get($"/{RegEnv.Env.Name}/CloudPlatform/Database/platform57/Password", true);
    }
    public class DbJobEndpoint : MysqlEndpoint
    {
        public DbJobEndpoint(RegEnv regenv, string endpoint) : base(regenv, endpoint)
        {
        }

        public override string Username => Ssm.Get($"/{RegEnv.Env.Name}/JobService/Database/jobapi57/Username", false);
        public override string Password => Ssm.Get($"/{RegEnv.Env.Name}/JobService/Database/jobapi57/Password", true);
    }
}