using System;
using System.Reflection;
using System.Reflection.Emit;

/*
    Keep in mind that when getting setting a field using its name, we have to look it up. This takes around 0.1 second for 1M lookups.
    Then we have to get/set the value per se. Reflection's Get/SetValue will add about 0.12 second for 1M ops. Not all that bad.
    Using an emitted delegate does 10 times faster though.
*/
    
namespace Coveo.Dal
{
    [Flags]
    public enum FieldFlags
    {
        IsPk = 0x01,
        IsRequired = 0x02,
        IsNotUseful = 0x04,
        IsFk = 0x08, // The physical *Id fields
        IsComputed = 0x10,
        IsEdge = 0x20,
        DontKeepType = 0x40,
    }

    public class MetaField
    {
        internal static Type TYPE_OBJECT = typeof(object);
        internal static Type[] TYPE_OBJECT_ARRAY = {typeof(object)};
        internal static Type[] TYPE_OBJECT_OBJECT_ARRAY = {typeof(object), typeof(object)};

        public static ConstructorInfo[] PrimNullableCtorInfos;

        internal FieldFlags _flags;

        public MetaType _metaType;
        public MetaClass MetaClass => (MetaClass)_metaType;
        public Type ClrType; // Either T in Nullable<T>, or FieldInfo.FieldType. Not orthogonal with getter, by design, because the poco is always a destination, from whatever external data coming in.
        public MetaType MetaType { get; private set; }
        public string Name => FieldInfo.Name;
        public FieldInfo FieldInfo;
        public GetFn GetFn;
        public SetFn SetFn;

        //public bool IsPk => (_flags & FieldFlags.IsPk) != 0;
        //public bool IsRequired => (_flags & FieldFlags.IsRequired) != 0;
        public bool IsNotUseful => (_flags & FieldFlags.IsNotUseful) != 0;
        //public bool IsFk => (_flags & FieldFlags.IsFk) != 0;

        static MetaField()
        {
            PrimNullableCtorInfos = new []
            {
                null, // Empty = 0,
                null, // Object = 1,
                null, // DBNull = 2
                typeof(bool?).GetConstructor(new[] {typeof(bool)}), // Boolean = 3
                typeof(char?).GetConstructor(new[] {typeof(char)}), // Char = 4
                typeof(sbyte?).GetConstructor(new[] {typeof(sbyte)}), // SByte = 5
                typeof(byte?).GetConstructor(new[] {typeof(byte)}), // Byte = 6
                typeof(short?).GetConstructor(new[] {typeof(short)}), // Int16 = 7
                typeof(ushort?).GetConstructor(new[] {typeof(ushort)}), // UInt16 = 8
                typeof(int?).GetConstructor(new[] {typeof(int)}), // Int32 = 9
                typeof(uint?).GetConstructor(new[] {typeof(uint)}), // UInt32 = 10
                typeof(long?).GetConstructor(new[] {typeof(long)}), // Int64 = 11
                typeof(ulong?).GetConstructor(new[] {typeof(ulong)}), // UInt64 = 12
                typeof(float?).GetConstructor(new[] {typeof(float)}), // Single = 13
                typeof(double?).GetConstructor(new[] {typeof(double)}), // Double = 14
                typeof(decimal?).GetConstructor(new[] {typeof(decimal)}), // Decimal = 15
                typeof(DateTime?).GetConstructor(new[] {typeof(DateTime)}) // DateTime = 16
            };
        }

        public MetaField(MetaType p_MetaType, FieldInfo p_FieldInfo, FieldFlags p_Flags = 0)
        {
            _metaType = p_MetaType;
            FieldInfo = p_FieldInfo;
            _flags = p_Flags;

            ClrType = FieldInfo.FieldType;
            ConstructorInfo ctorInfo = null;
            if (ClrType.Name == "Nullable`1") {
                var underlyingType = Nullable.GetUnderlyingType(ClrType) ?? throw new DalException("Unexpected null underlying type.");
                ctorInfo = underlyingType.IsEnum
                    ? ClrType.GetConstructor(new[] {underlyingType})
                    : PrimNullableCtorInfos[(int)Type.GetTypeCode(underlyingType)];
                ClrType = underlyingType;
            }

            GetFn = CreateGetter();
            SetFn = CreateSetter(ctorInfo);
            
            MetaType = _metaType.Meta.TypeToMetaType(ClrType);
        }

        public GetFn CreateGetter()
        {
            // object GetFld(object obj) { return (yyy)((Pocoxxx)obj).fld; }
            var fi = FieldInfo;
            var m = new DynamicMethod("Get" + fi.Name, TYPE_OBJECT, TYPE_OBJECT_ARRAY);
            var il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, _metaType.ClrType);
            il.Emit(OpCodes.Ldfld, fi);
            il.Emit(OpCodes.Box, fi.FieldType);
            il.Emit(OpCodes.Ret);
            return (GetFn)m.CreateDelegate(typeof(GetFn));
        }

        public SetFn CreateSetter(ConstructorInfo p_CtorInfo)
        {
            // void SetFld(object obj, object val) { ((Pocoxxx)obj).fld = (yyy)val; }
            var m = new DynamicMethod("Set" + FieldInfo.Name, null, TYPE_OBJECT_OBJECT_ARRAY);
            var il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, _metaType.ClrType);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, ClrType); // "Operation could destabilize the runtime" if using only 'Unbox', because reference types are not handled.
            if (p_CtorInfo != null)
                il.Emit(OpCodes.Newobj, p_CtorInfo);
            il.Emit(OpCodes.Stfld, FieldInfo);
            il.Emit(OpCodes.Ret);
            return (SetFn)m.CreateDelegate(typeof(SetFn));
        }
        
        internal void SetMetaType(MetaType p_FieldType)
        {
            MetaType = p_FieldType;
            if (MetaType is MetaDomain)
                MetaType.AddRef(this);
        }

        public override string ToString()
        {
            return $"{MetaClass.TypeName}.{Name}";
        }
    }
}