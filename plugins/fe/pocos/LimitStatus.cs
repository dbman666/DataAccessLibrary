using System;
using System.Collections.Generic;
using System.Linq;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.LimitStatus")]
    public class LimitStatus
    {
        public string LimitKey;
        public long? LimitValue;
        [OrgId] public string OrganizationId;
        public string Section;

        [Pk][Computed] public string FullPk;

        [Edge_Org_LimitStatus] public Organization Organization;
        
        public void PostLoad()
        {
            FullPk = string.Join(Ctes.SEP_PK2, OrganizationId, Section, LimitKey);
        }
    }

    public class LimitStatusForOrg
    {
        public string OrgId;
        public SectionContent content;
        public SectionOrganization organization;
        public SectionSearchApi searchApi;
        public SectionUsageAnalytics usageAnalytics;

        public LimitStatusForOrg(string orgId)
        {
            OrgId = orgId;
        }

        public void Add(LimitStatus limit)
        {
            var clz = (MetaClass)Meta.G.FindOrCreateMetaType(typeof(LimitStatusForOrg));
            var fld = clz.FindMetaField(limit.Section);
            if (fld == null) throw new DalException($"LimitStatusForOrg: Section '{limit.Section}' is not a known field.");
            var section = fld.GetFn(this);
            if (section == null) {
                section = fld.MetaType.CtorFn();
                fld.SetFn(this, section);
            }
            
            var clz2 = (MetaClass)Meta.G.FindOrCreateMetaType(fld.ClrType);
            var fld2 = clz2.FindMetaField(limit.LimitKey);
            if (fld2 == null) throw new DalException($"LimitStatusForOrg: Section '{limit.Section}' does not contain the field '{limit.LimitKey}'.");
            fld2.SetFn(section, limit.LimitValue);
        }

        public static LimitStatusForOrg Convert(string p_OrgId, List<LimitStatus> p_Limits)
        {
            if (p_Limits.Count == 0)
                return null;
            var limitStatusForOrg = new LimitStatusForOrg(p_OrgId);
            foreach (var limit in p_Limits)
                limitStatusForOrg.Add(limit);
            return limitStatusForOrg;
        }
    }

    public class LimitHistoryForOrg
    {
        public string OrgId;
        public DateTime Date;
        public SectionContent Content;
        public SectionSearchApi SearchApi;
        
        public LimitHistoryForOrg(string orgId, DateTime date)
        {
            OrgId = orgId;
            Date = date;
        }

        public void Add(LimitHistory limit)
        {
            var clz = (MetaClass)Meta.G.FindOrCreateMetaType(typeof(LimitHistoryForOrg));
            var fld = clz.FindMetaField(limit.Section);
            if (fld == null) throw new DalException($"LimitStatusForOrg: Section '{limit.Section}' is not a known field.");
            var section = fld.GetFn(this);
            if (section == null) {
                section = fld.MetaType.CtorFn();
                fld.SetFn(this, section);
            }
            
            var clz2 = (MetaClass)Meta.G.FindOrCreateMetaType(fld.ClrType);
            var fld2 = clz2.FindMetaField(limit.LimitKey);
            if (fld2 == null) throw new DalException($"LimitStatusForOrg: Section '{limit.Section}' does not contain the field '{limit.LimitKey}'.");
            fld2.SetFn(section, limit.LimitValue);
        }

        public static List<LimitHistoryForOrg> Convert(string p_OrgId, List<LimitHistory> p_Limits)
        {
            if (p_Limits.Count == 0)
                return null;
            var map = new Dictionary<DateTime, LimitHistoryForOrg>();
            foreach (var limit in p_Limits) {
                if (!map.TryGetValue(limit.Date, out var limitForOrg)) {
                    limitForOrg = new LimitHistoryForOrg(p_OrgId, limit.Date);
                    map[limit.Date] = limitForOrg;
                }
                limitForOrg.Add(limit);
            }
            return map.Values.OrderBy(l => l.Date).ToList();
        }
    }
    
    public class SectionContent
    {
        public long? licenseExtensionsLimit;
        public long? numberOfDocumentsLimit;
        public long? numberOfFieldsLimit;
        public long? numberOfSecurityProvidersLimit;
        public long? numberOfSourcesLimit;
        public long? pushApiMaximumDailyInvocations;
        public long? pushApiMaximumDailyDocumentCount;
        public long? pushApiMaximumDailyDocumentLimit;
        public long? pushApiMaximumDailyRelationshipLimit;
        public long? numberOfOcrPagesLimit;
    }
    
    public class SectionOrganization
    {
        public long? numberOfApiKeysLimit;
        public long? numberOfDailyEmailNotificationsSentPerOrganizationLimit;
        public long? numberOfGroupsLimit;
        public long? numberOfSubscriptionsPerOrganizationLimit;
    }
    
    public class SectionSearchApi
    {
        public long? agentUsersPerMonthLimit;
        public long? distinctInternalUserLimit;
        public long? distinctPartnerUserLimit;
        public long? distinctPowerPartnerUserLimit;
        public long? distinctStaticQueriesLimit;
        public long? distinctUserLimit;
        public long? intranetUsersPerMonthLimit;
        public long? partnerUsersPerMonthLimit;
        public long? queriesPerMonthLimit;
        public long? recommendationsPerMonthLimit;
        public long? staticQueriesPerMonthLimit;
        public long? generalActivityLimit;
        public long? searchCountLimit;
        public long? recommendationCountLimit;
    }
    
    public class SectionUsageAnalytics
    {
        public long? customDimensionLimit;
        public long? dailyEventsLimit;
        public long? exportLimit;
        public long? exportScheduleLimit;
        public long? monthlyEventsLimit;
        public long? namedFilterLimit;
        public long? permissionFilterLimit;
        public long? queriesPerHourLimit;
        public long? queriesPerMinuteLimit;
        public long? reportLimit;
    }
}