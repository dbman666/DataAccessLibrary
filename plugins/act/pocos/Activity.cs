using System;
using System.Collections.Generic;
using fe;
using Coveo.Dal;

namespace act
{
    public class Activity
    {
        [Pk] [ActivityId] public string id;
        [OrgId] public string organizationId;
        public Section section;
        public ResourceType resourceType;
        public Operation operation;
        public State state;
        public string resourceId;
        public string resourceName;
        public DateTime createdDate;
        public DateTime? startDate;
        public DateTime? endDate;
        public Result result;
        public string snapshotId;
        public Content content;
        //computedSorting
        public long version;
        public DateTime? updatedDate;
        public double progress;
        public TriggeredBy triggerBy;

        [Computed] public ResultType? resultType;
        [Computed] public string resultErrorCode;
        [Computed] public TimeSpan duration;
        [Computed] [OperationId] public string contentOperationId;

        [Edge_Task_Activity] public List<Task_Source> Tasks;
        [Edge_SourceState_Activity] public SourceState SourceState;

        public void PostLoad()
        {
            resultType = result?.type;
            resultErrorCode = result?.errorCode;

            if (startDate.HasValue) {
                var ts = (endDate.HasValue ? endDate.Value : DateTime.UtcNow) - startDate.Value;
                duration = new TimeSpan(0, 0, (int)ts.TotalSeconds); // Chop off micros.
            } else
                duration = new TimeSpan();

            contentOperationId = content?.operationId;
        }
    }
}