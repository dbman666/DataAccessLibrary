using System.Reflection;

namespace Coveo.Dal
{
    public class MetaEdgeField : MetaField
    {
        internal MetaEdge _metaEdge;
        internal bool _isFrom;
        internal bool _is1;

        public MetaEdge MetaEdge => _metaEdge;
        public bool IsFrom => _isFrom;
        public bool IsTo => !_isFrom;
        public bool Is1 => _is1;
        public MetaField Fk { get; }
        public MetaEdgeField OtherField { get; internal set; }

        public MetaEdgeField(MetaClass p_MetaClass, FieldInfo p_FieldInfo, bool p_Is1, MetaField p_Fk, FieldFlags p_Flags = 0) : base(p_MetaClass, p_FieldInfo, p_Flags | FieldFlags.IsEdge)
        {
            _is1 = p_Is1;
            Fk = p_Fk;
            if (p_Fk != null)
                p_Fk._flags |= FieldFlags.IsFk;
        }
    }
}