using System;
using Coveo.Dal;

namespace fe
{
    [Table(null)]
    public class BackupEntry
    {
        public DateTime? CreatedDate;
        public string DTYPE;
        [Pk] public string Id;
        public string Location;
        [OrgId] public string OrganizationId;
        [IndexId] public string ResourceId;
        public string Bucket;

        [Edge_Org_BackupEntry] public Organization Organization;
    }
}