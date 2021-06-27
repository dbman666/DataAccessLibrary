using System.Collections.Generic;

namespace Coveo.Dal
{
    public class MetaIndex
    {
        public MetaClass _onType;
        public bool Unique { get; }
        public List<MetaField> Fields; // Worth having many ? Hmm maybe for duplicate detection, but that would be a validation. So it would just be for fast access. Also, no Asc/Desc ?

        public MetaIndex(MetaClass p_OnType, bool p_Unique = true)
        {
            _onType = p_OnType;
            Unique = p_Unique;
        }
    }
}