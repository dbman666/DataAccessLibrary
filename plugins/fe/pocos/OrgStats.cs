using System;

namespace fe
{
    [Flags]
    public enum StatsKind
    {
// @formatter:off
        NbSourcesPerOrg             = 0x0001,
        NbFieldsPerOrg              = 0x0002,
        NbSecProvsPerOrg            = 0x0004,
        NbDocsPerOrg                = 0x0008,
        NbDailyQueriesPerOrg        = 0x0010,
        NbDailyPushApiCallsPerOrg   = 0x0020,
        SizeDailyPushApiCallsPerOrg = 0x0040,
        NbMappingsPerSource         = 0x0080,
        NbExtensionsPerSource       = 0x0100,
        All                         = 0xFFFF,
// @formatter:on
    }

    public static class DefaultLimits
    {
        public static long NbSourcesPerOrg = 10;
        public static long NbFieldsPerOrg = 5000;
        public static long NbSecProvsPerOrg = 1; //15;
        public static long NbDocsPerOrg = 100000;
        public static long NbDailyQueriesPerOrg = 50000;
        public static long NbDailyPushApiCallsPerOrg = 300000;
        public static long SizeDailyPushApiCallsPerOrg = 256000000;
        public static long NbMappingsPerSource = 2500;
        public static long NbExtensionsPerSource = 20;
    }

    public partial class OrgStats
    {
        public SingleStat NbSources;
        public SingleStat NbFields;
        public SingleStat NbSecProvs;
        public SingleStat NbDocs;
        public SingleStat NbDailyQueries;
        public SingleStat NbDailyPushApiCalls;
        public SingleStat SizeDailyPushApiCalls;
    }

    public partial class SourceStats
    {
        public SingleStat NbMappings;
        public SingleStat NbExtensions;
    }

    public partial class SingleStat
    {
        public long Actual;
        public long Limit;
        public int Percent;

        public SingleStat(long p_Actual, long p_Limit)
        {
            Actual = p_Actual;
            Limit = p_Limit;
            Percent = Limit == 0 ? 0 : (int)((double)p_Actual / p_Limit * 100);
        }
    }
}