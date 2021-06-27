namespace Coveo.Dal
{
    public class AwsCredentials
    {
        public string AccessKey;
        public string SecretKey;
        public string Token;

        public static AwsCredentials This => new AwsCredentials {AccessKey = Var.Get("AWS_ACCESS_KEY_ID"), SecretKey = Var.Get("AWS_SECRET_ACCESS_KEY"), Token = Var.Get("AWS_SESSION_TOKEN")};
    }
}