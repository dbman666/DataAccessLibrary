using System;

namespace Coveo.Dal
{
    public static class Var
    {
        public static string Get(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception("Missing env var " + name);
    }
}