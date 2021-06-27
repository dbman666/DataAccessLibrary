using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.LicenseTemplate")]
    public class LicenseTemplate : Versioned
    {
        public string AdditionalInfo;
        [StringList] public string AllowedSourceTypes; // todo-poco - And they should match another enum
        public string AllowedSourceConfigurations; // todo-poco - And they should match another enum
        public string Department;
        public LicenseIndexType? IndexType;
        public MonitoringLevel? MonitoringLevel;
        [LicenseTemplateName] [Pk] public string Name;
        public LicenseProductEdition? ProductEdition;
        public LicenseProductName? ProductName;
        public LicenseProductType? ProductType;
        public LicenseIndexBackupType? IndexBackupType;
        [Json] public string Properties;
        public bool? PublicTemplate;
        public int? ValidityPeriodInDays;
        [Json] public List<string> PrivilegeMaskIds;

        [Edge_License_LicenseTemplate] public List<License> Licenses;
        [Edge_LicenseTemplate_OrgTemplate] public List<OrganizationTemplate> OrgTemplates;

        private Row _propertiesAsRow;

        public Row PropertiesAsRow(Repo p_Repo)
        {
            if (_propertiesAsRow == null)
#pragma warning disable 618
                _propertiesAsRow = p_Repo.ParseJsonAsRow(string.Copy(Properties));
#pragma warning restore 618
            return _propertiesAsRow;
        }
    }
}