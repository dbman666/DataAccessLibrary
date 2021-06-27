using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.LimitHistory")]
    public class LimitHistory
    {
        public DateTime Date;
        public string LimitKey;
        public long LimitValue;
        [OrgId] public string OrganizationId;
        public string Section;

        [Pk][Computed] public string FullPk;

        [Edge_Org_LimitHistory] public Organization Organization;

        public void PostLoad()
        {
            FullPk = string.Join(Ctes.SEP_PK2, OrganizationId, Date, Section, LimitKey);
        }
    }
}