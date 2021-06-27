using System;

namespace act
{
    public class Result
    {
        public ResultType? type;
        public string errorCode;
        public string errorDetail;
        public string abortReason;
        public ActivityAbortedBy abortedBy;
        public DateTime lastUpdatedDate;
    }
}