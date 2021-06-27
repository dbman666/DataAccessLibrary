using System;
using System.Collections.Generic;
using Amazon.SimpleSystemsManagement;

namespace Coveo.Dal
{
    public static class Clients
    {
        public static Dictionary<string, object> Cache = new();

        public static T FromCacheOrElse<T>(string key, Func<T> funcElse)
        {
            if (!Cache.TryGetValue(key, out var val)) {
                val = funcElse();
                Cache[key] = val;
            }
            return (T)val;
        }

        public static AmazonSimpleSystemsManagementClient SsmClient => FromCacheOrElse("ssm", () => new AmazonSimpleSystemsManagementClient(Reg.This.AwsRegion));

        public static MysqlProxy DbFeClient => FromCacheOrElse("db.fe", () => RegEnv.This.DbFeEndpoint.Client);

        public static MysqlProxy DbJobClient => FromCacheOrElse("db.job", () => RegEnv.This.DbJobEndpoint.Client);
    }
}