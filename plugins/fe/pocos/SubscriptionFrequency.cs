namespace fe
{
    public enum SubscriptionFrequency
    {
        LIVE,
        HOURLY,
        DAILY,
        WEEKLY
    }

//{
//    LIVE
//    {
//        @Override
//        public String getCronExpression()
//        {
//            return "* * * * * ?";
//        }
//
//        @Override
//        public DateTime getPreviousTriggerDate()
//        {
//            return DateTime.now();
//        }
//    },
//    HOURLY
//    {
//        @Override
//        public String getCronExpression()
//        {
//            return "0 0 */0 * * ?";
//        }
//
//        @Override
//        public DateTime getPreviousTriggerDate()
//        {
//            return DateTime.now().minusHours(1);
//        }
//    },
//    DAILY
//    {
//        @Override
//        public String getCronExpression()
//        {
//            return "0 0 0 * * ? *";
//        }
//
//        @Override
//        public DateTime getPreviousTriggerDate()
//        {
//            return DateTime.now().minusDays(1);
//        }
//    },
//    WEEKLY
//    {
//        @Override
//        public String getCronExpression()
//        {
//            return "0 0 0 ? * SUNDAY";
//        }
//
//        @Override
//        public DateTime getPreviousTriggerDate()
//        {
//            return DateTime.now().minusWeeks(1);
//        }
//    };
}
