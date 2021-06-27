using Coveo.Dal;

namespace fe
{
    [Table("NotificationService.DefaultSubscription")]
    public class DefaultSubscription
    {
        [Json] public string Content;
        public string Description;
        public SubscriptionFrequency? Frequency;
        [DefaultSubscriptionId] [Pk] public string Id;
        public string Name;
        [StringSet] public string Operations;
        public string Parameters;
        [StringSet] public string ResourceTypes;
        [StringSet] public string ResultTypes;
        public SubscriptionType? Type;
    }
}