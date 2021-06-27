using Coveo.Dal;

namespace fe
{
    [Table(null)]
    public class Schedule
    {
        public int? DayOfTheMonth;
        [StringList] public string DaysOfWeek;
        public ScheduleDtype? DTYPE;
        public bool Enabled;
        public int? Hour;
        public int? HourOccurrence;
        [Pk] public string Id;
        public int? Minute;
        public int? MinuteOccurrence;
        public int? MonthOccurrence;
        [OrgId] public string OrganizationId;
        public OperationType RefreshType;
        public string ResourceId; // see Fk* below
        public ScheduleFrequencyType ScheduleFrequencyType;
        public ScheduleType ScheduleType;
        public int? Seed;

        [Computed] public string HumanCron;
        [Computed] public string FkSourceId;
        [Computed] public string FkSecCacheId;
        [Computed] public string FkSecProvId;

        [Edge_Org_Schedule] public Organization Organization;

        public void PostLoad()
        {
            if (ScheduleType == ScheduleType.SOURCE)
                FkSourceId = ResourceId;
            if (ScheduleType == ScheduleType.SECURITY_CACHE)
                FkSecCacheId = ResourceId;
            if (ScheduleType == ScheduleType.SECURITY_PROVIDER)
                FkSecProvId = SecurityProvider.ComputedFullId(OrganizationId, ResourceId);
            
            HumanCron = ToSched().ToHtml();
        }
        
        public Sched ToSched()
        {
            return new Sched
            {
                Enabled = Enabled,
                RefreshType = RefreshType,
                FrequencyType = ScheduleFrequencyType,
                HourOccurrence = HourOccurrence,
                MinuteOccurrence = MinuteOccurrence,
                MonthOccurrence = MonthOccurrence,
                DaysOfWeek = DaysOfWeek,
                DayOfTheMonth = DayOfTheMonth,
                Hour = Hour,
                Minute = Minute
            };
        }
    }
}