using fe;
using Coveo.Dal;

namespace job
{
    public class JobPlugin : Plugin
    {
        public static JobPlugin G = new JobPlugin(Meta.G);

        public JobPlugin(Meta p_Meta) : base(p_Meta, "job", new[] {typeof(FePlugin)}, new[] {typeof(Job), typeof(JobStatus)})
        {
        }
    }
}