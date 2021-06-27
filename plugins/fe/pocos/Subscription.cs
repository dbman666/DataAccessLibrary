using System.Collections.Generic;
using Coveo.Dal;

namespace fe
{
    [Table("NotificationService.Subscription")]
    public class Subscription : Versioned
    {
        [Json] public string Content;
        public string Description;
        public bool? Enabled;
        public SubscriptionFrequency? Frequency;
        [SubscriptionId] [Pk] public string Id;
        public string Name;
        [StringSet] public string Operations;
        [OrgId] public string OrganizationId;
        [Json] public SubscriptionParameters Parameters;
        [StringSet] public string ResourceTypes;
        [StringSet] public string ResultTypes;
        public SubscriptionType? Type;
        [Email] public string UserId;

        [Edge_Org_Subscription] public Organization Organization;
    }

    public class SubscriptionParameters
    {
        public List<string> emailRecipients; 
        public string serviceUrl;
        public string fromDisplayName;
        public string emailSubject;
    }
}