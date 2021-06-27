using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Coveo.Dal
{
    public class Meta
    {
        public static Meta G = new Meta();

        internal UniqueNames _fieldNames;
        internal bool _caseInsensitiveNames;

        public List<Plugin> Plugins { get; } = new List<Plugin>();
        public MetaType[] PrimTypes { get; }
        public TableColType[] PrimTableColTypes { get; }
        public Dictionary<string, TableColType> TableColTypes { get; } = new Dictionary<string, TableColType>(StringComparer.OrdinalIgnoreCase);
        public TableColType ColTypeForEnum;

        public List<MetaEdge> Edges { get; } = new List<MetaEdge>();

        // This dictionary is just used to find existing poco classes. See MetaType.ctor to see that we skip primitive types.
        public Dictionary<Type, MetaType> MetaTypes { get; } = new Dictionary<Type, MetaType>();

        internal Meta(bool p_CaseInsensitiveNames = true)
        {
            _caseInsensitiveNames = p_CaseInsensitiveNames;
            _fieldNames = new UniqueNames(_caseInsensitiveNames);

            PrimTypes = new MetaType[]
            {
                null, // Empty = 0
                null, // Object = 1
                new PrimType(this, typeof(DBNull), "void"), // DBNull = 2
                new BoolType(this), // Boolean = 3
                new NumType(this, typeof(char), "char"), // Char = 4
                new NumType(this, typeof(sbyte), "sbyte"), // SByte = 5
                new NumType(this, typeof(byte), "byte"), // Byte = 6
                new NumType(this, typeof(short), "short"), // Int16 = 7
                new NumType(this, typeof(ushort), "ushort"), // UInt16 = 8
                new NumType(this, typeof(int), "int"), // Int32 = 9
                new NumType(this, typeof(uint), "uint"), // UInt32 = 10
                new NumType(this, typeof(long), "long"), // Int64 = 11
                new NumType(this, typeof(ulong), "ulong"), // UInt64 = 12
                new NumType(this, typeof(float), "float"), // Single = 13
                new NumType(this, typeof(double), "double"), // Double = 14
                null, // Decimal = 15
                new DateTimeType(this), // DateTime = 16
                null,
                new StringType(this), // String = 18
            };
            {var _ = new TimeSpanType(this);}
            //_ = new MetaType(this, typeof(Row), null, false);

            // Formatters
            var numFmt = new NumColType(this);
            PrimTableColTypes = new TableColType[]
            {
                null, // Empty = 0
                null, // Object = 1
                null, // DBNull = 2
                new BoolColType(this), // Boolean = 3
                numFmt, // Char = 4
                numFmt, // SByte = 5
                numFmt, // Byte = 6
                numFmt, // Int16 = 7
                numFmt, // UInt16 = 8
                numFmt, // Int32 = 9
                numFmt, // UInt32 = 10
                numFmt, // Int64 = 11
                numFmt, // UInt64 = 12
                numFmt, // Single = 13
                numFmt, // Double = 14
                null, // Decimal = 15
                new DateTimeColType(this), // DateTime = 16
                null,
                new StringColType(this), // String = 18
            };
            {var _ = new TimeSpanColType(this);}
            ColTypeForEnum = new EnumColType(this);
            {var _ = new DataSizeColType(this);}
        }

        public MetaType TypeToMetaType(Type p_Type)
        {
            return PrimTypes[(int)Type.GetTypeCode(p_Type)];
        }

        public TableColType TypeToTableColType(Type p_Type)
        {
            return PrimTableColTypes[(int)Type.GetTypeCode(p_Type)];
        }

        public int FindField(string p_Name)
        {
            return _fieldNames.Find(p_Name);
        }

        public void Add(Plugin p_Plugin)
        {
            Plugins.Add(p_Plugin);
        }

        public MetaType FindTypeByName(string p_Name)
        {
            foreach (var type in MetaTypes.Values)
                if (type.TypeName == p_Name)
                    return type;
            return null;
        }

        public MetaEdge FindEdge(string p_Name)
        {
            foreach (var edge in Edges)
                if (edge.Name == p_Name)
                    return edge;
            return null;
        }

        public MetaEdge FindEdge(Type p_EdgeType)
        {
            foreach (var edge in Edges)
                if (edge.EdgeType == p_EdgeType)
                    return edge;
            return null;
        }

        public List<MetaEdge> FindEdges(Type p_FromType, Type p_ToType)
        {
            List<MetaEdge> edges = null;
            foreach (var edge in Edges)
                if (edge.FromField.MetaClass.ClrType == p_FromType && edge.ToField.MetaClass.ClrType == p_ToType) {
                    edges ??= new();
                    edges.Add(edge);
                }
            return edges;
        }

        public MetaEdge Add(MetaEdge p_Edge)
        {
            if (p_Edge.Meta != null) CmfUtil.Throw($"Type '{p_Edge}' already attached to Meta '{p_Edge.Meta}'");
            Edges.Add(p_Edge);
            p_Edge.Meta = this;
            return p_Edge;
        }

        public MetaType AddMetaType(MetaType p_MetaType)
        {
            if (MetaTypes.ContainsKey(p_MetaType.ClrType)) throw new DalException($"Type '{p_MetaType.TypeName}' :: '{p_MetaType.ClrType.Name}' already in meta.");
            MetaTypes[p_MetaType.ClrType] = p_MetaType;
            return p_MetaType;
        }

        public MetaType FindMetaType(Type p_Type)
        {
            return MetaTypes.TryGetValue(p_Type, out var metaType) ? metaType : null;
        }

        public MetaType FindOrCreateMetaType(Type p_Type)
        {
            if (MetaTypes.TryGetValue(p_Type, out var metaType))
                return metaType;
            return p_Type.IsClass ? new MetaClass(this, p_Type, null) : new MetaType(this, p_Type);
        }

        public void Add(TableColType p_ColType)
        {
            var name = p_ColType.Name;
            if (TableColTypes.ContainsKey(name)) throw new DalException($"Meta.Add: TableColType '{name}' already exists.");
            TableColTypes[name] = p_ColType;
        }

        public TableColType FindTableColType(string p_Name)
        {
            return TableColTypes.TryGetValue(p_Name, out var colType) ? colType : null;
        }

        public void ValidateEdges()
        {
            foreach (var edge in Edges) {
                if (edge.FromField == null)
                    Console.WriteLine($"Edge '{edge.Name}' is missing its 'from' field.");
                if (edge.ToField == null)
                    Console.WriteLine($"Edge '{edge.Name}' is missing its 'to' field.");
                if (edge.FromField != null && edge.ToField != null)
                    if (edge.FromField.Fk == null && edge.ToField.Fk == null)
                        Console.WriteLine($"Edge '{edge.Name}' has no fkField for neither from nor to.");
            }
        }

        public MetaClass GetMetaClass(object p_Row)
        {
            return (MetaClass)FindMetaType(p_Row.GetType());
        }

        public static FieldInfo[] GetFieldsOrderedByClassHierarchy(Type type)
        {
            var src = type.GetFields();
            var nb = src.Length;
            if (nb == 0)
                return src;
            var dst = new FieldInfo[nb];
            var iSrc = 0;
            // ReSharper disable once CoVariantArrayConversion
            foreach (var group in GetMemberGroups(src)) {
                Array.Copy(src, group.Item1, dst, iSrc, group.Item2);
                iSrc += group.Item2;
            }
            return dst;
        }

        public static PropertyInfo[] GetPropertiesOrderedByClassHierarchy(Type type)
        {
            var src = type.GetProperties();
            var nb = src.Length;
            if (nb == 0)
                return src;
            var dst = new PropertyInfo[nb];
            var iSrc = 0;
            // ReSharper disable once CoVariantArrayConversion
            foreach (var group in GetMemberGroups(src)) {
                Array.Copy(src, group.Item1, dst, iSrc, group.Item2);
                iSrc += group.Item2;
            }
            return dst;
        }

        private static IEnumerable<(int, int)> GetMemberGroups(MemberInfo[] members)
        {
            var groups = members.GroupBy(m => m.DeclaringType).Select(g => (g.Key, nb: g.Count())).ToArray();
            var iGroup = members.Length;
            var i = groups.Length;
            while (--i >= 0) {
                var group = groups[i];
                iGroup -= group.nb;
                yield return (iGroup, group.nb);
            }
        }
    }
}