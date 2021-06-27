using System.Collections.Generic;
using fe;

namespace act
{
    public class Content
    {
        [OperationId] public string operationId;
        public List<string> addedPrivilegeMasks;
        public List<string> removedPrivilegeMasks;
        public List<string> unchangedPrivilegeMasks;
        public string organizationName;
        public string ownerDisplayName;
        public string licenseTemplateName;
        public string ownerEmail;
        public string sourceId;
        public string copyFromId;
    }
}