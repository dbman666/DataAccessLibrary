namespace Coveo.Dal
{
    public abstract class MysqlEndpoint
    {
        public string Endpoint;
        public string UsernameSsmKey;
        public string PasswordSsmKey;
        public string Username => Ssm.Get(UsernameSsmKey, false);
        public string Password => Ssm.Get(PasswordSsmKey, true);

        public MysqlProxy Client => new MysqlProxy(Endpoint, Username, Password);
    }

    public class DbFeEndpoint : MysqlEndpoint
    {
    }
    public class DbJobEndpoint : MysqlEndpoint
    {
    }
}