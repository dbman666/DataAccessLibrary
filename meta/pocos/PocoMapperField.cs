using System;

namespace Coveo.Dal
{
    public class PocoMapperField
    {
        public string SrcName;
        public Type SrcType;
        public int IdxInPoco;
        public MetaField MetaField;
        public ConvFn ConvFn;
    }
}