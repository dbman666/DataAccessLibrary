using System;
using System.Text;

namespace Coveo.Dal
{
    public class TableColType
    {
        public string Name;

        public TableColType(Meta p_Meta, string p_Name)
        {
            Name = p_Name;
            p_Meta.Add(this);
        }

        public virtual void AppendJsonValue(StringBuilder sb, object p_Data) => sb.Append($"\"{p_Data}\"");

        public virtual string GetTdCell(PocoField pf, object data) => $"<td>{data}</td>";

        public virtual string ToDataSort(object p_Data) => null;

        public virtual string GetColClass() => null;

        public virtual bool IsNullOrDefault(object p_Data)
        {
            if (p_Data == null)
                return true;
            switch (Type.GetTypeCode(p_Data.GetType())) {
            case TypeCode.Boolean: return (bool)p_Data == default(bool);
            case TypeCode.SByte: return (sbyte)p_Data == default(sbyte);
            case TypeCode.Byte: return (byte)p_Data == default(byte);
            case TypeCode.Char: return (char)p_Data == default(char);
            case TypeCode.Int16: return (short)p_Data == default(short);
            case TypeCode.UInt16: return (ushort)p_Data == default(ushort);
            case TypeCode.Int32: return (int)p_Data == default(int);
            case TypeCode.UInt32: return (uint)p_Data == default(uint);
            case TypeCode.Int64: return (long)p_Data == default(long);
            case TypeCode.UInt64: return (ulong)p_Data == default(ulong);
            case TypeCode.Single: return (float)p_Data == default(float);
            case TypeCode.Double: return (double)p_Data == default(double);
            case TypeCode.DateTime: return (DateTime)p_Data == default(DateTime);
            case TypeCode.String: return string.IsNullOrWhiteSpace((string)p_Data);
            }
            return false;
        }
    }

    public class BoolColType : TableColType
    {
        public BoolColType(Meta p_Meta) : base(p_Meta, "bool")
        {
        }

        public override void AppendJsonValue(StringBuilder sb, object p_Data) => sb.Append((bool)p_Data ? Ctes.TRUE : Ctes.FALSE);
    }

    public class NumColType : TableColType
    {
        public NumColType(Meta p_Meta) : base(p_Meta, "num")
        {
        }

        public override void AppendJsonValue(StringBuilder sb, object p_Data) => sb.Append(p_Data);
        public override string GetColClass() => "num";
    }

    public class StringColType : TableColType
    {
        public StringColType(Meta p_Meta) : base(p_Meta, "string")
        {
        }

        public override void AppendJsonValue(StringBuilder sb, object p_Data) => JsonParserBase.EscapeStringIfNeeded(sb, p_Data.ToString(), true);
    }

    public class DateTimeColType : TableColType
    {
        public DateTimeColType(Meta p_Meta) : base(p_Meta, "DateTime")
        {
        }

        public override void AppendJsonValue(StringBuilder sb, object p_Data) => sb.Append($"\"{p_Data:u}\"");
        public override string ToDataSort(object p_Data) => $"\"{CmfUtil.MillisSinceEpoch((DateTime)p_Data)}\"";
    }

    public class TimeSpanColType : TableColType
    {
        public TimeSpanColType(Meta p_Meta) : base(p_Meta, "TimeSpan")
        {
        }
    }

    public class EnumColType : TableColType
    {
        public EnumColType(Meta p_Meta) : base(p_Meta, "enum")
        {
        }
        
        public override string GetTdCell(PocoField pf, object data)
        {
            var metaEnum = (MetaEnum)Meta.G.FindMetaType(pf.Type);
            var metaEnumVal = metaEnum.Values[(int)data];
            return metaEnumVal.CssColor == null ? $"<td>{data}</td>" : $"<td style=\"background-color:{metaEnumVal.CssColor};\">{data}</td>";
        }
    }

    public class DataSizeColType : TableColType
    {
        public DataSizeColType(Meta p_Meta) : base(p_Meta, "DataSize")
        {
        }

        public override void AppendJsonValue(StringBuilder sb, object p_Data) => sb.Append($"\"{CmfUtil.ToSizeString((ulong)p_Data)}\"");
        public override string ToDataSort(object p_Data) => $"\"{(ulong)p_Data}\"";
        public override string GetColClass() => "num";
    }
}