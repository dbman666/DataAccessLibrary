using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Coveo.Dal
{
    public class MetaEdge
    {
        public static List<Exception> EMPTY_EXCEPTIONS = new List<Exception>();
        
        public Meta Meta { get; internal set; }
        public Type EdgeType { get; }
        public string Name { get; }
        public MetaEdgeField FromField { get; internal set; }
        public MetaEdgeField ToField { get; internal set; }

        //public static MetaEdge New(Meta p_Meta, string p_EdgeName,
        //        MetaClass p_FromType, out MetaEdgeField p_FromField, string p_FromFieldName, bool p_FromIs1,
        //        MetaClass p_ToType, out MetaEdgeField p_ToField, string p_ToFieldName, bool p_ToIs1, MetaField p_ToFkField)
        //{
        //    p_FromField = p_FromType.Add(new MetaEdgeField(p_FromType, p_FromIs1, null));
        //    p_ToField = p_ToType.Add(new MetaEdgeField(p_ToType, p_ToIs1, p_ToFkField));
        //    return new MetaEdge(p_Meta, p_EdgeName, p_FromField, p_ToField);
        //}

        public MetaEdge(Meta p_Meta, string p_Name, MetaEdgeField p_FromField, MetaEdgeField p_ToField)
        {
            if (p_FromField == null || p_ToField == null) throw new DalException($"MetaEdge.ctor: Unexpected null fields for edge '{p_Name}'.");
            Name = p_Name;
            FromField = p_FromField;
            ToField = p_ToField;
            p_FromField._metaEdge = this;
            p_FromField._isFrom = true;
            p_ToField._metaEdge = this;
            p_FromField.OtherField = ToField;
            p_ToField.OtherField = p_FromField;
            p_Meta.Add(this);
        }

        public MetaEdge(Meta p_Meta, Type p_EdgeType, string p_Name)
        {
            EdgeType = p_EdgeType;
            Name = p_Name;
            p_Meta.Add(this);
        }

        internal MetaEdgeField NewEdgeField(MetaClass p_MetaClass, FieldInfo p_FieldInfo, bool p_Is1, MetaField p_FkField)
        {
            var isFrom = p_FkField == null; // 'to' is always on the fk side. 
            var edgeField = new MetaEdgeField(p_MetaClass, p_FieldInfo, p_Is1, p_FkField);
            edgeField._metaEdge = this;
            edgeField._isFrom = isFrom;
            if (isFrom) {
                if (ToField != null) {
                    edgeField.OtherField = ToField;
                    ToField.OtherField = edgeField;
                }
                FromField = edgeField;
            } else {
                if (FromField != null) {
                    edgeField.OtherField = FromField;
                    FromField.OtherField = edgeField;
                }
                ToField = edgeField;
            }
            return edgeField;
        }

        public List<Exception> CreateEdges(Repo p_Repo)
        {
            List<Exception> exceptions = null;
            if (p_Repo.ContainsEdge(this)) throw new DalException($"Edge '{Name}' has already not been materialized.");
            Table fromTable = p_Repo.GetLoadedTable(FromField.MetaClass) ?? throw new DalException($"CreateEdges: Missing 'from' table '{FromField.MetaClass.TypeName}'.");
            Table toTable = p_Repo.GetLoadedTable(ToField.MetaClass) ?? throw new DalException($"CreateEdges: Missing 'to' table '{ToField.MetaClass.TypeName}'.");
            var toClass = toTable.MetaClass;
            if (FromField.Fk != null) throw new DalException("Expected only one side of the edge to have a companion fk field.");
            var fkField = ToField.Fk ?? throw new DalException($"Expected '{ToField.Name}' to have a companion fk field.");
            if (!ToField.Is1) throw new DalException($"Expected '{ToField.Name}' to have a card of 1.");
            var fromIs1 = FromField.Is1;

            // Initialize all lists to an empty one, to ease navigation.
            // All non-loaded tables will have null-lists, and will throw if used, which makes sense.
            if (!fromIs1)
                foreach (object fromRow in fromTable)
                    FromField.SetFn(fromRow, toClass.NewList());

            foreach (object toRow in toTable) {
                var fk = fkField.GetFn(toRow);
                if (fk == null) continue;
                if (!(fk is string fkStr)) throw new DalException("Expected fk field to be a string.");
                if (fromTable.RowsByPk.TryGetValue(fkStr, out var fromRow)) {
                    ToField.SetFn(toRow, fromRow);
                    if (fromIs1) {
                        if (FromField.GetFn(fromRow) != null) throw new DalException("Expected from is1 to be null.");
                        FromField.SetFn(fromRow, toRow);
                    } else {
                        var list = (IList)FromField.GetFn(fromRow);
                        if (list == null) throw new DalException("CreateEdges: The list should already have been created.");
                        list.Add(toRow);
                    }
                } else {
                    AddException(ref exceptions, new DalException($"Pk '{fkStr}' not found in table '{FromField.MetaClass.TypeName}'."));
                }
            }
            p_Repo.Add(this);
            return exceptions ?? EMPTY_EXCEPTIONS;
        }

        private void AddException(ref List<Exception> p_Exceptions, Exception p_Exc)
        {
            if (p_Exceptions == null)
                p_Exceptions = new List<Exception>();
            p_Exceptions.Add(p_Exc);
        }

        public bool AreTablesLoaded(Repo p_Repo)
        {
            return FromField != null && ToField != null && p_Repo.GetLoadedTable(FromField.MetaClass) != null && p_Repo.GetLoadedTable(ToField.MetaClass) != null;
        }
    }
}