using System.Collections.Generic;
using Amazon.SimpleSystemsManagement.Model;

namespace Coveo.Dal
{
    public static class Ssm
    {
        public static Dictionary<string, string> Cache = new();

        public static string Get(string key, bool decrypt)
        {
            if (!Cache.TryGetValue(key, out var val)) {
                val = Clients.SsmClient.GetParameterAsync(new GetParameterRequest {Name = key, WithDecryption = decrypt}).Result.Parameter.Value;
                //(T)System.Convert.ChangeType(val, typeof(T));
                Cache[key] = val;
            }
            return val;
        }
    }
}