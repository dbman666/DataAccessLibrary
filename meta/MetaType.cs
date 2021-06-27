using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

// For JsonParser, test the efficiency of first checking the 'name' of the 'current' position's name (first char, then name), vs systematically doing the lookup. For always-in-the-same-order fields, that could do a huge difference.

// For question of why couldn't conversions take place in the IL directly:
//  1- Convert.ChangeType is very efficient (like only 3 to 4 times slower than hard-coded conversion)
//  2- It could be done only when the srcType is known in advance (DataReader), and NOT when it's json. Plus the IL would have to be re-done in the wrapper, not the MetaField.
//  3- The ConvFn would have to be called for stuff like enums, datetimes, anyway.
//  4- The boxing of strings would have to be done anyway. This could be done in the IL though, but we'd have to pass the IPocoRepo to the 'Set' :(

namespace Coveo.Dal
{
    public delegate object CtorFn();

    public delegate object GetFn(object obj);

    public delegate void SetFn(object obj, object val);

    public class MetaType
    {
        public static readonly object FALSE = false;
        public static readonly object TRUE = true;

        private static List<MetaField> NO_REF_FIELDS = new List<MetaField>();

        public Meta Meta { get; }
        public Type ClrType { get; }
        public TypeCode ClrTypeCode { get; }
        public string TypeName { get; }
        public MetaType ParentType { get; }

        // Class-specific.
        // To have that in PocoClass means MetaClass wouldn't be able to 'extend' PocoClass and MetaType at the same time. So we keep this all in one class, like .Net's 'Type'.
        protected int _nbParent;
        protected int _nb;
        public FieldInfo[] FieldInfos { get; }
        public int[] FieldNameIdxs { get; }
        public MetaField[] Fields { get; }
        public CtorFn CtorFn;

        private List<MetaField> _refFields;

        public List<MetaField> RefFields => _refFields ?? NO_REF_FIELDS;
        public virtual bool IsPrimNum => false;

        public MetaType(Meta p_Meta, Type p_ClrType, string p_Name = null, bool p_CreateMetaFields = true)
        {
            Meta = p_Meta;
            ClrType = p_ClrType;
            ClrTypeCode = Type.GetTypeCode(p_ClrType);
            TypeName = p_Name ?? GetName(ClrType);
            p_Meta.AddMetaType(this);

            if (ClrTypeCode == TypeCode.Object && p_CreateMetaFields) {
                var baseType = p_ClrType.BaseType;
                if (baseType != typeof(object) && baseType != typeof(ValueType) && baseType != null && !(baseType.IsSubclassOf(typeof(Attribute))))
                    ParentType = p_Meta.FindOrCreateMetaType(p_ClrType.BaseType);
                CtorFn = CreateCtor();
                var fis = ClrType.GetFields(BindingFlags.Instance | BindingFlags.Public); // Careful: the ones from the parent are at the end. Hence the gymnastics.
                _nb = fis.Length;
                _nbParent = ParentType == null ? 0 : ParentType.FieldInfos.Length;
                Fields = new MetaField[_nb];
                FieldNameIdxs = new int[_nb];
                if (ParentType == null) {
                    FieldInfos = fis;
                } else {
                    FieldInfos = new FieldInfo[_nb];
                    Array.Copy(ParentType.FieldInfos, 0, FieldInfos, 0, _nbParent);
                    Array.Copy(fis, 0, FieldInfos, _nbParent, _nb - _nbParent);
                    Array.Copy(ParentType.FieldNameIdxs, 0, FieldNameIdxs, 0, _nbParent);
                    Array.Copy(ParentType.Fields, 0, Fields, 0, _nbParent);
                }
                for (var i = _nbParent; i < _nb; ++i)
                    FieldNameIdxs[i] = Meta._fieldNames.Find(FieldInfos[i].Name, true);

                //for (var i = nbParent; i < nb; ++i)
                //    Fields[i] = new MetaField(this, FieldInfos[i]);
            }
        }

        public CtorFn CreateCtor()
        {
            // object New() { return new xxx; }
            var ctor = ClrType.GetConstructor(Type.EmptyTypes);
            if (ctor == null) // Array for instance, string[], ...
                return null;
            //throw new DalException($"Type '{ClrType.Name}' has no default ctor.");
            var m = new DynamicMethod("New" + ClrType.Name, MetaField.TYPE_OBJECT, Type.EmptyTypes);
            var il = m.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);
            return (CtorFn)m.CreateDelegate(typeof(CtorFn));
        }

        public static string GetName(Type p_Type)
        {
            var name = p_Type.Name;
            var pos = name.IndexOf('`');
            if (pos == -1)
                return name;
            name = name.Substring(0, pos);
            name += '<';
            var genTypes = p_Type.GenericTypeArguments;
            for (int i = 0; i < genTypes.Length; ++i) {
                if (i > 0) name += Ctes.SEP_COMMA_SPACE;
                name += GetName(genTypes[i]);
            }
            name += '>';
            return name;
        }

        public object New()
        {
            return CtorFn();
        }

        public MetaField FindMetaField(string p_Name)
        {
            var nameIdx = Meta.FindField(p_Name);
            var idxInPoco = Array.IndexOf(FieldNameIdxs, nameIdx);
            return idxInPoco == -1 ? null : Fields[idxInPoco];
        }

        public virtual bool IsValid(object p_Value)
        {
            return true;
        }

        internal void AddRef(MetaField p_Field)
        {
            _refFields ??= new();
            _refFields.Add(p_Field);
        }

        public virtual object EnumFromName(string p_Name)
        {
            throw new NotImplementedException();
        }

        public unsafe int Compare(object p_Val1, object p_Val2)
        {
            // Take care of the null values, so we only compare non-nulls.
            var isNull1 = p_Val1 == null;
            var isNull2 = p_Val2 == null;
            if (isNull1) return isNull2 ? 0 : -1;
            if (isNull2) return -1;

            var tc1 = p_Val1 is IConvertible ic1 ? ic1.GetTypeCode() : TypeCode.Object;
            var tc2 = p_Val2 is IConvertible ic2 ? ic2.GetTypeCode() : TypeCode.Object;
            if (tc1 != tc2) throw new DalException($"MetaType.Compare: Expected the 2 values to have the same TypeCode. Got {tc1} vs {tc2}.");

            // @formatter:off
            switch (tc1) {
            case TypeCode.Boolean   : { var v1 = (bool    )p_Val1; var v2 = (bool    )p_Val2; return (v1 == v2 ? 0 : v2      ? -1 : 1); }
            case TypeCode.SByte     : { var v1 = (sbyte   )p_Val1; var v2 = (sbyte   )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Byte      : { var v1 = (byte    )p_Val1; var v2 = (byte    )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Char      : { var v1 = (char    )p_Val1; var v2 = (char    )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Int16     : { var v1 = (short   )p_Val1; var v2 = (short   )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.UInt16    : { var v1 = (ushort  )p_Val1; var v2 = (ushort  )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Int32     : { var v1 = (int     )p_Val1; var v2 = (int     )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.UInt32    : { var v1 = (uint    )p_Val1; var v2 = (uint    )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Int64     : { var v1 = (long    )p_Val1; var v2 = (long    )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.UInt64    : { var v1 = (ulong   )p_Val1; var v2 = (ulong   )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Single    : { var v1 = (float   )p_Val1; var v2 = (float   )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.Double    : { var v1 = (double  )p_Val1; var v2 = (double  )p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.DateTime  : { var v1 = (DateTime)p_Val1; var v2 = (DateTime)p_Val2; return (v1 == v2 ? 0 : v1 < v2 ? -1 : 1); }
            case TypeCode.String    : return string.Compare((string)p_Val1, (string)p_Val2, StringComparison.Ordinal); // For the delta comparisons, at least.
            case TypeCode.Object    : {
                if (p_Val1 is byte[] v1) {
                    var v2 = (byte[])p_Val2;
                    fixed (byte* p1 = v1, p2 = v2)
                        return new ReadOnlySpan<byte>(p1, v1.Length).SequenceCompareTo(new ReadOnlySpan<byte>(p2, v2.Length));
                }
                if (p_Val1 is TimeSpan ts1) {
                    var ts2 = (TimeSpan)p_Val2;
                    return (ts1 == ts2 ? 0 : ts1 < ts2 ? -1 : 1);
                }
                break;
            }
            }
            // @formatter:on
            throw new DalException($"MetaType.Compare: Invalid TypeCode '{tc1}' ({p_Val1.GetType()}).");
        }
    }
}