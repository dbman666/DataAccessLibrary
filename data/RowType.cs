using System.Collections.Generic;

namespace Coveo.Dal
{
    public class RowType
    {
        public Meta Meta;
        public string Name;
        public List<int> FieldNameIds = new List<int>();

        public int AddFieldUnlessFound(int p_FieldId, MetaType p_FieldType, List<Row> p_ExistingRows)
        {
            var rowIdx = FieldNameIds.IndexOf(p_FieldId);
            if (rowIdx == -1) {
                rowIdx = FieldNameIds.Count;
                FieldNameIds.Add(p_FieldId);
                foreach (var r in p_ExistingRows)
                    r._values.Add(null);
            }
            return rowIdx;
        }
    }
}