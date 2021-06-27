using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Coveo.Dal
{
    public class PocoTable
    {
        public PocoTableEvents _pocoTableEvents;
        public string Id;
        public int Depth;
        public IEnumerable Rows;
        public PocoTableHeaders Headers;
        public string ClassName;
        public PocoMatrix PocoMatrix;

        private MetaClass _metaClass;
        private PropertyInfo _pkPropInfo;
        private FieldInfo _pkFieldInfo;

        public PocoTable(PocoTableEvents p_PocoTableEvents, string p_Id, int p_Depth, IEnumerable p_Rows, bool p_TriggerEventIfEmpty = true)
        {
            _pocoTableEvents = p_PocoTableEvents;
            Id = p_Id;
            Depth = p_Depth;
            Rows = p_Rows;
            Headers = new PocoTableHeaders(p_Rows.GetType());
            ClassName = ExtractClassName(Headers.OfField.Type);
            PocoMatrix = new PocoMatrix(Headers.FlatFields);
            var rowIdExtractor = GetRowIdExtractor();
            foreach (var row in Rows)
                if (row != null) {
                    var pocoRow = PocoMatrix.AddRow(rowIdExtractor(row));
                    ExtractPocoMatrixInternal(pocoRow, Headers.OfField, row);
                }
            if (p_TriggerEventIfEmpty || PocoMatrix.DataMatrix.Count != 0)
                _pocoTableEvents.OnNewTable(this);
        }

        private string ExtractClassName(Type p_Type)
        {
            var name = p_Type.FullName ?? "";
            if (name.StartsWith("<>")) {
                // "<>f__AnonymousType1`6[..." 
                var pos = name.IndexOf(Ctes.CHAR_OPEN_SQUARE);
                return name.Substring(5, pos - 5).Replace(Ctes.CHAR_BACK_QUOTE, Ctes.CHAR_UNDERSCORE);
            }
            return name.Replace(Ctes.CHAR_DOT, Ctes.CHAR_UNDERSCORE);
        }

        private Func<object, string> GetRowIdExtractor()
        {
            // Not so nice to refer to Meta.G here.
            var metaType = Meta.G.FindMetaType(Headers.OfField.Type);
            if (metaType != null) {
                _metaClass = metaType as MetaClass;
                if (_metaClass != null && _metaClass.Pk != null)
                    return GetPkFromMetaClass;
                return GetObjAsPk;
            }
            _pkPropInfo = Headers.OfField.Type.GetProperty("_pk");
            if (_pkPropInfo != null)
                return GetPkFromPropName;
            _pkFieldInfo = Headers.OfField.Type.GetField("_pk");
            if (_pkFieldInfo != null)
                return GetPkFromFieldName;
            //throw new DalException($"GetRowIdExtractor: Don't know how to extract the pk from type '{Headers.OfField.Type.Name}'.");
            Console.WriteLine($"GetRowIdExtractor: Don't know how to extract the pk from type '{Headers.OfField.Type.Name}'.");
            return GetObjAsPk;
        }

        private string GetPkFromMetaClass(object p_Row)
        {
            return (string)_metaClass.Pk.Field.GetFn(p_Row);
        }
        
        private string GetObjAsPk(object p_Row)
        {
            return p_Row.ToString();
        }
        
        private string GetPkFromPropName(object p_Row)
        {
            return (string)_pkPropInfo.GetValue(p_Row);
        }
        
        private string GetPkFromFieldName(object p_Row)
        {
            return (string)_pkFieldInfo.GetValue(p_Row);
        }
        
        private void ExtractPocoMatrixInternal(PocoRow p_PocoRow, PocoField p_Field, object p_Data)
        {
            if (p_Data == null)
                return;
            if (p_Field.IsList) {
                var newTable = new PocoTable(_pocoTableEvents, $"{Id}_{p_PocoRow.Id}_{p_Field.Name}", Depth + 1, (IEnumerable)p_Data, false);
                if (newTable.PocoMatrix.DataMatrix.Count != 0)
                    PocoMatrix.Add(p_PocoRow, p_Field.Id, newTable);
            } else if (p_Field.IsClass) {
                foreach (var field in p_Field.SubFields)
                    ExtractPocoMatrixInternal(p_PocoRow, field, field.GetValue(p_Data));
            } else {
                PocoMatrix.Add(p_PocoRow, p_Field.Id, p_Data);
            }
        }
        
        public List<int> CompareRows(PocoRow p_Row1, PocoRow p_Row2)
        {
            var flatFields = Headers.FlatFields;
            List<int> unequalFields = null;
            for (int i = 0; i < flatFields.Count; ++i) {
                var data1 = p_Row1.Values[i];
                var data2 = p_Row2.Values[i];
                if (!Equals(data1, data2)) {
                    unequalFields ??= new();
                    unequalFields.Add(i);
                }
            }
            return unequalFields;
        }
    }
    
    public class PocoTableHeaders
    {
        public Type Type;
        public int NbRows;
        public int NbCols;
        public PocoField OfField;
        public List<List<PocoField>> Matrix = new List<List<PocoField>>();
        public List<PocoField> FlatFields;

        public PocoTableHeaders(Type p_Type)
        {
            Type = p_Type;
            var ofType = p_Type.GenericTypeArguments[p_Type.BaseType?.Name == "Iterator`1" ? 1 : 0];
            OfField = BuildListField(0, 0, ofType.Name, ofType);
            FlatFields = BuildFlatFields();
            int fieldId = 0;
            foreach (var field in FlatFields)
                field.Id = fieldId++;
        }

        private void Add(int p_Row, int p_Col, PocoField p_Field)
        {
            if (p_Col == NbCols) {
                // Repeat last column.
                var lastCol = NbCols - 1;
                for (var iRow = 0; iRow < NbRows; ++iRow) {
                    var row = Matrix[iRow];
                    row.Add(iRow < p_Row ? row[lastCol] : null);
                }
                ++NbCols;
                if (NbCols == 2000)
                    Debugger.Break();
            }
            if (p_Row == NbRows) {
                var newRow = new List<PocoField>();
                for (var iCol = 0; iCol < NbCols; ++iCol)
                    newRow.Add(null);
                Matrix.Add(newRow);
                ++NbRows;
            }
            Matrix[p_Row][p_Col] = p_Field;
        }

        private PocoField BuildListField(int p_Row, int p_Col, string p_Name, Type p_Type)
        {
            var pf = new PocoField(p_Name, p_Type);
            BuildFieldInfo(p_Row, p_Col, pf);
            return pf;
        }

        private List<PocoField> BuildFields(int p_Row, int p_Col, Type p_Type)
        {
            var fields = new List<PocoField>();
            var props = Meta.GetPropertiesOrderedByClassHierarchy(p_Type);
            if (props.Length == 0) {
                foreach (var field in Meta.GetFieldsOrderedByClassHierarchy(p_Type)) {
                    if (SkipField(field)) continue;
                    var pf = new PocoField(field);
                    fields.Add(pf);
                    BuildFieldInfo(p_Row, p_Col, pf);
                    p_Col = NbCols;
                }
            } else {
                foreach (var prop in props) {
                    if (SkipProp(prop)) continue;
                    var pf = new PocoField(prop);
                    fields.Add(pf);
                    BuildFieldInfo(p_Row, p_Col, pf);
                    p_Col = NbCols;
                }
            }
            return fields;
        }

        private bool SkipField(FieldInfo p_FieldInfo)
        {
            foreach (var a in p_FieldInfo.GetCustomAttributes()) {
                switch (a) {
                case JsonAttribute _:
                    if (p_FieldInfo.FieldType == typeof(string))
                        return true;
                    break;
                case IsNotUsefulAttribute _:
                case EdgeAttribute _:
                    return true;
                }
            }
            return p_FieldInfo.Name == "_pk";
        }

        private bool SkipProp(PropertyInfo p_PropInfo)
        {
            return p_PropInfo.Name == "_pk";
        }

        private void BuildFieldInfo(int p_Row, int p_Col, PocoField pf)
        {
            Add(p_Row, p_Col, pf);
            var name = pf.Name;
            var type = pf.Type;
            var typeName = type.Name;
            pf.ColType = Meta.G.TypeToTableColType(type); // See below for the handling of types like TimeSpan.
            // Handle the <fieldname>_format_<metatypename> case.
            var pos = name.IndexOf("_format_");
            if (pos != -1) {
                pf.ColType = CheckColType(name.Substring(pos + 8));
                pf.Name = name.Substring(0, pos);
            }
            pf.IsString = type == typeof(string);
            if (pf.IsString)
                return;
            if (typeName == "Nullable`1") {
                var underlyingType = Nullable.GetUnderlyingType(type) ?? throw new DalException("Unexpected null underlying type.");
                pf.IsNullable = true;
                pf.Type = type = underlyingType;
                if (pf.ColType == null) // May have been specified with a '_format_'.
                    pf.ColType = Meta.G.TypeToTableColType(type);
                typeName = type.Name;
            }
            pf.IsEnum = type.IsEnum;
            if (pf.IsEnum)
                pf.ColType = Meta.G.ColTypeForEnum; // Because the first lookup returned 'NumColType'.
            pf.IsNum = pf.ColType is NumColType;
            if (pf.IsNum)
                return;
            if (typeName[0] == '<' && typeName[1] == '>') {
                pf.IsAnonymous = true;
                pf.IsClass = true;
                pf.SubFields = new List<PocoField>(BuildFields(p_Row + 1, p_Col, type));
            } else if (typeName == "List`1") {
                pf.IsList = true;
                //pf.OfField = BuildListField(p_Row + 1, p_Col, pf.Name + "[]", type.GenericTypeArguments[0]);
            } else if (typeName == "IEnumerable`1") {
                pf.IsList = true;
                //pf.OfField = BuildListField(p_Row, p_Col, pf.Name + "[]", type.GenericTypeArguments[0]);
            } else if (type.IsClass && type != typeof(object)) {
                pf.IsClass = true;
                pf.SubFields = new List<PocoField>(BuildFields(p_Row + 1, p_Col, type));
            } else if (pf.ColType == null) {
                pf.ColType = CheckColType(typeName);
            }
        }

        private TableColType CheckColType(string p_Name)
        {
            return Meta.G.FindTableColType(p_Name) ?? throw new DalException($"PocoTable '{Type.Name}': TableColType for '{p_Name}' is not found. Could you have a Dictionary<> ?");
        }

        private List<PocoField> BuildFlatFields()
        {
            var flatFields = new List<PocoField>();
            var lastRow = NbRows - 1;
            for (var col = 0; col < NbCols; ++col)
                for (var iRow = lastRow; iRow >= 0; --iRow)
                    if (Matrix[iRow][col] != null) {
                        flatFields.Add(Matrix[iRow][col]);
                        break;
                    }
            return flatFields;
        }

        public int GetNbNonNullHeaderRows(List<int> p_WhichCols)
        {
            var iRow = Matrix.Count;
            while (iRow-- > 0) {
                var row = Matrix[iRow];
                foreach (var col in p_WhichCols)
                    if (row[col] != null)
                        return iRow + 1;
            }
            throw new DalException("TableFields.GetNbNonNullHeaderRows: All rows would be null ? Nah.");
        }

        public bool SameAs(PocoTableHeaders p_Headers2)
        {
            if (NbCols != p_Headers2.NbCols)
                return false;
            for (int i = 0; i < NbCols; ++i)
                if (FlatFields[i].Name != p_Headers2.FlatFields[i].Name)
                    return false;
            return true;
        }
    }

    public partial class PocoField
    {
        public PropertyInfo PropInfo;
        public FieldInfo FieldInfo;
        public string Name;
        public int Id;
        public Type Type;
        public TableColType ColType;
        //public MetaType MetaType;
        public PocoField OfField;
        public bool IsString;
        public bool IsNum;
        public bool IsEnum;
        public bool IsNullable;
        public bool IsList;
        public bool IsClass;
        public bool IsAnonymous;
        public List<PocoField> SubFields;

        public PocoField(string p_Name, Type p_Type)
        {
            Name = p_Name;
            Type = p_Type;
        }

        public PocoField(PropertyInfo p_Prop)
        {
            PropInfo = p_Prop;
            Name = p_Prop.Name;
            Type = p_Prop.PropertyType;
        }

        public PocoField(FieldInfo p_Field)
        {
            FieldInfo = p_Field;
            Name = p_Field.Name;
            Type = p_Field.FieldType;
        }

        public object GetValue(object p_Row)
        {
            return p_Row == null ? null : PropInfo == null ? FieldInfo.GetValue(p_Row) : PropInfo.GetValue(p_Row);
        }
        
        public bool IsNullOrDefault(object p_Data)
        {
            if (p_Data == null)
                return true;
            if (IsEnum)
                return false;
            return ColType == null ? false : ColType.IsNullOrDefault(p_Data);
        }

        public override string ToString()
        {
            return Name;
        }
    }
    
    public class PocoMatrix
    {
        public List<PocoField> Fields;
        public int NbCols;
        public List<PocoRow> DataMatrix;
        public bool[] HasOnlyNulls;

        public PocoMatrix(List<PocoField> p_Fields)
        {
            Fields = p_Fields;
            NbCols = p_Fields.Count;
            DataMatrix = new List<PocoRow>();
            HasOnlyNulls = new bool[NbCols];
            for (int col = 0; col < NbCols; ++col)
                HasOnlyNulls[col] = true;
        }

        public PocoRow AddRow(string p_Id)
        {
            var pocoRow = new PocoRow(p_Id, new object[NbCols]);
            DataMatrix.Add(pocoRow);
            return pocoRow;
        }

        public void Add(PocoRow p_PocoRow, int p_Col, object p_Data)
        {
            if (p_Data != null && !Fields[p_Col].IsNullOrDefault(p_Data)) {
                p_PocoRow.Values[p_Col] = p_Data;
                HasOnlyNulls[p_Col] = false;
            }
        }

        public bool AllNulls(int p_Col)
        {
            return HasOnlyNulls[p_Col];
        }
    }

    public class PocoRow
    {
        public string Id;
        public object[] Values;

        public PocoRow(string p_Id, object[] p_Values)
        {
            Id = CleanupId(p_Id);
            Values = p_Values;
        }

        public static string CleanupId(string p_Id)
        {
            // Quite sure there are other problematic chars, because we use this id to lookup ids using jquery.
            // 1- I had changed my separator from '|' to '_'.
            // 2- I do it for rows, because even if table ids were not found, they are actually the path leading to the tables ({pk + fieldname}).
            // 3- Of course row pks are usually not a problem. It's for secprovs I first noticed the problem.
            return p_Id?.Replace(Ctes.CHAR_SPACE, Ctes.SEP_PK)
                .Replace(Ctes.CHAR_DOT, Ctes.SEP_PK)
                .Replace(Ctes.CHAR_SLASH, Ctes.SEP_PK)
                .Replace(Ctes.CHAR_COLON, Ctes.SEP_PK);
        }
    }

    public interface PocoTableEvents
    {
        void OnNewTable(PocoTable p_PocoTable);
    }
}