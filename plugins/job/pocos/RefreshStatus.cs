using System;
using Coveo.Dal;

namespace job
{
    public enum Type
    {
        None,
        Rebuild,
        FullRefresh,
        IncrementalRefresh,
        RefreshUri,
    }

    public enum Status
    {
        NotStarted,
        Refreshing,
        Paused,
        Interrupted,
        Stopped,
        Finished,
        OnError,
        PausedOnError,
    }

    public class RefreshStatus
    {
        [Pk] public string Id;
        public DateTime Ts;
        public string RowState;
        public string name;
        public string anyStatus;
        public int sourceId;
        public int collectionId;
        public string operationId;
        public Type type;
        public Status status;
        public DateTime startDate;
        public DateTime endDate;
        public int addedCount;
        public int updatedCount;
        public int removedCount;
        public int unchangedCount;
        public int filteredCount;
        public long totalSize;
        public string indexIdentifier;
        public RefreshError error;
        public DateTime LastActivityDateUtc;
        public int IgnoredUriCount;

        // SecCache-specific
        public int NumberOfCreatedJobs;
        public int NumberOfSuccessfulJobs;
        public int NumberOfProcessedJobs;
        public int NumberOfFailedJobs;
        public int NumberOfSkippedJobs;
    }

    public class RefreshError
    {
        public string message;
        public string code;
    }
}