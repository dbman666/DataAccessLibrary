using System;
using Coveo.Dal;

namespace fe
{
    [Table("PlatformService.License")]
    public class License : Versioned
    {
        public string AccountId;
        public string AccountName;
        public string AdditionalInfo;
        [Json] [IsNotUseful] public string Connectors;
        public string Department;
        public DateTime? ExpirationDate;
        public LicenseIndexType? IndexType;
        public MonitoringLevel? MonitoringLevel;
        [OrgId] [Pk(true)] public string OrganizationId;
        public LicenseProductEdition? ProductEdition;
        public LicenseProductName? ProductName;
        public LicenseProductType? ProductType;
        public LicenseIndexBackupType? IndexBackupType;
        [Json] public string Properties;
        public string Type;
        [IsNotUseful] public long MaximumPushApiFileSize;

        [Edge_Org_License] public Organization Organization;
        [Edge_License_LicenseTemplate(FkFieldName = "type")] public LicenseTemplate Template;

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