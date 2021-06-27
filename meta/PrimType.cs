using System;

namespace Coveo.Dal
{
    public class PrimType : MetaType
    {
        public PrimType(Meta p_Meta, Type p_RawType, string p_ClrName = null) : base(p_Meta, p_RawType, p_ClrName)
        {
        }
    }

    public class BoolType : PrimType
    {
        public BoolType(Meta p_Meta) : base(p_Meta, typeof(bool), "bool")
        {
        }
    }

    public class NumType : PrimType
    {
        public NumType(Meta p_Meta, Type p_Type, string p_Name) : base(p_Meta, p_Type, p_Name)
        {
        }
    }

    public class StringType : PrimType
    {
        public StringType(Meta p_Meta) : base(p_Meta, typeof(string), "string")
        {
        }
    }

    public class DateTimeType : PrimType
    {
        public DateTimeType(Meta p_Meta) : base(p_Meta, typeof(DateTime))
        {
        }
    }

    public class TimeSpanType : PrimType
    {
        public TimeSpanType(Meta p_Meta) : base(p_Meta, typeof(TimeSpan))
        {
        }
    }
}