namespace act
{
    public class ActivityAbortedBy
    {
        public string displayName;
        public string userId;
        public TriggeredByType abortedByType;
        public string relatedActivityId;
        public Operation relatedActivity;
        public string relatedActivityResourceType;
    }
}