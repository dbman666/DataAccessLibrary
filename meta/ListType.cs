using System;

namespace Coveo.Dal
{
    public class ListType : MetaType
    {
        public MetaType OfType { get; }

        public ListType(Meta p_Meta, Type p_ClrType, MetaType p_OfType) : base(p_Meta, p_ClrType)
        {
            OfType = p_OfType;
        }
    }
}