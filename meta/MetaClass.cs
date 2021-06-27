using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Coveo.Dal
{
    public class MetaClass : MetaType
    {
        // If no 'pre-defined' names, then the objects are totally dynamic.
        // If there are FieldNames, then we compute the FieldNameIds from them, and when building Rows,
        // the first FieldNames.Length values in Row.Values are for them. The rest can be added dynamically.
        // Fields are optional. Useful if we want to constrain the type.

        private const FieldFlags SKIPPABLE = FieldFlags.IsPk | FieldFlags.IsNotUseful | FieldFlags.IsFk;
        private static Type[] ARGS_TYPE_REPO = {typeof(Repo)};
        public static object[] EMPTY_ARGS = new object[0];

        internal ConstructorInfo _tableCtorInfo;
        internal MethodInfo _postLoadMethodInfo;
        internal ConstructorInfo _listCtorInfo;
        internal MethodInfo _postLoadTableMethodInfo;
        internal MethodInfo _postEdgesMethodInfo;

        public MetaType[] FieldTypes => Fields.Select(f => f.MetaType).ToArray();
        public MetaPk Pk { get; }
        public TableAttribute TableAttribute { get; }

        protected internal MetaClass(Meta p_Meta, Type p_ClrType, string p_Name) : base(p_Meta, p_ClrType, p_Name)
        {
            for (var i = _nbParent; i < _nb; ++i) {
                var fi = FieldInfos[i];
                var flags = (FieldFlags)0;
                PkAttribute pka = null;
                MetaType newFieldType = null;
                EdgeAttribute ea = null;

                foreach (var a in fi.GetCustomAttributes()) {
                    switch (a) {
                    case DomainAttribute da:
                        newFieldType = ExtractDomain(da);
                        break;
                    case PkAttribute _pka:
                        if (pka != null) throw new DalException($"More than one [Pk]  on '{fi}'.");
                        pka = _pka;
                        break;
                    case ComputedAttribute _:
                        flags |= FieldFlags.IsComputed;
                        break;
                    case DontKeepTypeAttribute _:
                        flags |= FieldFlags.DontKeepType;
                        break;
                    case IsNotUsefulAttribute _:
                        flags |= FieldFlags.IsNotUseful;
                        break;
                    case EdgeAttribute _ea:
                        if (ea != null) throw new DalException($"More than one [Edge*]  on '{fi}'.");
                        ea = _ea;
                        break;
                    }
                }

                var mtField = ea == null ? new MetaField(this, fi, flags) : ExtractEdgeInfo(fi, ea);
                if (mtField.ClrType.IsEnum)
                    newFieldType = p_Meta.FindMetaType(mtField.ClrType) ?? new MetaEnum(p_Meta, mtField.ClrType);
                if (newFieldType == null && mtField.MetaType == null && (flags & FieldFlags.DontKeepType) == 0) {
                    newFieldType = p_Meta.FindOrCreateMetaType(mtField.ClrType);
                    if (newFieldType == null) {
                        throw new DalException($"Can't map type '{mtField.FieldInfo.FieldType.Name}' to a MetaType.");
                        //Console.WriteLine($"Type '{mtField.PrimType.Name}' is not mapped.");
                    }
                }
                Fields[i] = mtField;
                
                if (pka != null)
                    Pk = new MetaPk(mtField, pka.IsDependent);
                if (newFieldType != null)
                    mtField.SetMetaType(newFieldType);
            }
            
            if (ParentType != null && Pk == null)
                Pk = ((MetaClass)ParentType).Pk;

            foreach (var a in p_ClrType.GetCustomAttributes())
                if (a is TableAttribute ta)
                    TableAttribute = ta;

            _postLoadMethodInfo = p_ClrType.GetMethod("PostLoad");
            _postEdgesMethodInfo = p_ClrType.GetMethod("PostEdges");

            _postLoadTableMethodInfo = p_ClrType.GetMethod("PostLoadTable", BindingFlags.Static | BindingFlags.Public, null, ARGS_TYPE_REPO, null);            
        }

        private MetaDomain ExtractDomain(DomainAttribute da)
        {
            var daType = da.GetType();
            var domainName = daType.Name;
            domainName = domainName.Substring(0, domainName.Length - 9);
            var domain = (MetaDomain)Meta.FindMetaType(daType);
            if (domain != null)
                return domain;
            //var baseAttrName = da.GetType().BaseType?.Name ?? "";
            //var baseType = baseAttrName.StartsWith("String") ? typeof(string) : (baseAttrName.StartsWith("Int") ? typeof(int) : throw new DalException($"Unexpected base domain attribute name '{baseAttrName}'"));
            return new MetaDomain(Meta, daType, domainName);
        }

        private MetaEdgeField ExtractEdgeInfo(FieldInfo fi, EdgeAttribute ea)
        {
            var edgeType = ea.GetType();
            var edgeName = edgeType.Name;
            if (!edgeName.StartsWith("Edge_")) throw new DalException($"Edge name should start with 'Edge_': {edgeName}");
            edgeName = edgeName.Substring(5, edgeName.Length - 14);
            var mtEdge = Meta.FindEdge(edgeName);
            if (mtEdge == null)
                mtEdge = new MetaEdge(Meta, edgeType, edgeName);
            var is1 = fi.FieldType.Name != "List`1";
            MetaField fkField = null;
            if (is1) {
                var fkFieldName = ea.FkFieldName ?? fi.Name + "Id";
                fkField = FindMetaField(fkFieldName);
                //if (fkField == null) throw new DalException($"Can't find fk field '{TypeName}.{fkFieldName}'");
            }
            //Console.WriteLine($"{edgeName}.{fkField?.Name}");
            return mtEdge.NewEdgeField(this, fi, is1, fkField);
        }

        // 1- Separate the Edge fields from the rest.
        // 2- Don't include the 'To' edge fields, because they link back to their parent.
        // 3- Always put the Pk fields first.
        public List<MetaField> ExtractAllExceptToFields(out List<MetaEdgeField> p_EdgeFields)
        {
            p_EdgeFields = new List<MetaEdgeField>();
            var ret = new List<MetaField>();
            ret.Add(Pk.Field);
            foreach (var field in Fields) {
                if (field is MetaEdgeField edgeField) {
                    if (edgeField.IsFrom)
                        p_EdgeFields.Add(edgeField);
                } else {
                    if ((field._flags & SKIPPABLE) == 0)
                        ret.Add(field);
                }
            }
            return ret;
        }

        public List<MetaField> TrimFieldsWithoutValues(List<MetaField> p_Fields, List<object> p_Rows)
        {
            var ret = new List<MetaField>();
            foreach (var field in p_Fields)
                if (HasSomeValues(field, p_Rows))
                    ret.Add(field);
            return ret;
        }

        public bool HasSomeValues(MetaField p_Field, List<object> p_Rows)
        {
            foreach (var row in p_Rows) {
                var val = p_Field.GetFn(row);
                if (val != null) {
                    if (val is string s && string.IsNullOrWhiteSpace(s))
                        return false;
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return TypeName;
        }

        public Table NewTable(Repo p_Repo)
        {
            if (_tableCtorInfo == null) {
                var newType = typeof(Table<>).MakeGenericType(ClrType);
                _tableCtorInfo = newType.GetConstructors()[0];
            }
            return (Table)_tableCtorInfo.Invoke(new object[] {p_Repo});
        }
        
        public virtual IList NewList()
        {
            if (_listCtorInfo == null) {
                var newType = typeof(List<>).MakeGenericType(ClrType);
                _listCtorInfo = newType.GetConstructors()[0];
            }
            return (IList)_listCtorInfo.Invoke(EMPTY_ARGS);
        }
        
        public void PostLoad(object p_Row)
        {
            if (_postLoadMethodInfo != null)
                _postLoadMethodInfo.Invoke(p_Row, EMPTY_ARGS);
        }
        
        public void PostEdges(object p_Row)
        {
            if (_postEdgesMethodInfo != null)
                _postEdgesMethodInfo.Invoke(p_Row, EMPTY_ARGS);
        }
        
        public string BuildPk(object p_Row)
        {
            return Pk == null ? p_Row.ToString() : (string)Pk.Field.GetFn(p_Row);
        }
        
        public List<int> CompareRows(object p_Row1, object p_Row2)
        {
            if (ClrType != p_Row1.GetType() || ClrType != p_Row2.GetType()) throw new DalException($"MetaClass.Compare: Expected matching types: {TypeName} vs {p_Row1.GetType().Name} vs {p_Row2.GetType().Name}.");
            List<int> unequalFields = null;
            for (int i = 0; i < Fields.Length; ++i) {
                var mf = Fields[i];
                if (mf is MetaEdgeField) continue;
                if (mf.MetaType == null) continue;
                if (mf.MetaType.Compare(mf.GetFn(p_Row1), mf.GetFn(p_Row2)) != 0) {
                    if (unequalFields == null)
                        unequalFields = new List<int>();
                    unequalFields.Add(i);
                }
            }
            return unequalFields;
        }
        
        public object[] GetValues(object p_Row, MetaField[] p_Fields)
        {
            var ret = new object[p_Fields.Length];
            int i = 0;
            foreach (var field in p_Fields)
                ret[i++] = field.GetFn(p_Row);
            return ret;
        }
        
        public int ExtractListsForFroms(object p_Row, List<MetaEdgeField> p_EdgeFields, out List<MetaEdgeField> p_EdgeFieldsWithData, out List<List<object>> p_EdgeFieldData)
        {
            p_EdgeFieldsWithData = null;
            p_EdgeFieldData = null;
            foreach (var edgeField in p_EdgeFields) {
                var child = edgeField.GetFn(p_Row);
                if (child == null) continue;
                var rowList = edgeField.Is1 ? new List<object> { child } : new List<object>((IEnumerable<object>)child);
                if (rowList.IsNullOrEmpty()) continue;
                if (p_EdgeFieldsWithData == null) {
                    p_EdgeFieldsWithData = new List<MetaEdgeField>();
                    p_EdgeFieldData = new List<List<object>>();
                }
                p_EdgeFieldsWithData.Add(edgeField);
                // ReSharper disable once PossibleNullReferenceException
                p_EdgeFieldData.Add(rowList);
            }
            return p_EdgeFieldsWithData == null ? 0 : p_EdgeFieldsWithData.Count;
        }
    }
}