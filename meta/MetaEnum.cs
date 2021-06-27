using System;
using System.Reflection;

namespace Coveo.Dal
{
    public class MetaEnumValue
    {
        public string Name;
        public int NameId;
        public int Value;
        public object Enum;
        public string CssColor;
    }

    public class MetaEnum : PrimType
    {
        public string[] _names;
        public int[] _fieldNameIds;

        public bool IsFlags { get; }
        public MetaEnumValue[] Values;

        public MetaEnum(Meta p_Meta, Type p_ClrType) : base(p_Meta, p_ClrType, p_ClrType.Name)
        {
            IsFlags = ClrType.IsDefined(typeof(FlagsAttribute), false);
            _names = Enum.GetNames(ClrType);
            var nb = _names.Length;
            _fieldNameIds = new int[nb];
            var values = Enum.GetValues(ClrType);
            Values = new MetaEnumValue[nb];
            var fieldInfos = p_ClrType.GetFields(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < nb; ++i) {
                var enm = values.GetValue(i) ?? throw new DalException("Impossible");
                var name = _names[i];
                var nameId = p_Meta._fieldNames.Find(name, true);
                _fieldNameIds[i] = nameId;
                Values[i] = new MetaEnumValue {Name = name, NameId = nameId, Value = (int)enm, Enum = enm };
                var cssColor = fieldInfos[i].GetCustomAttribute<CssColorAttribute>();
                if (cssColor != null)
                    Values[i].CssColor = cssColor.Color;
            }
        }

        public override object EnumFromName(string p_Name)
        {
            var nameId = Meta._fieldNames.Find(p_Name);
            var idx = nameId == -1 ? -1 : Array.IndexOf(_fieldNameIds, nameId);
            if (idx == -1)
                throw new DalException($"MetaEnum:EnumFromName: String '{p_Name}' can't be converted to enum {TypeName}.");
            return Values[idx].Enum;
        }
    }
}