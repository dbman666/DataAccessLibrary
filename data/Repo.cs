using System;
using System.Collections.Generic;
using System.Linq;

namespace Coveo.Dal
{
    public partial class Repo
    {
        internal UniqueStrings _strings;
        internal Meta _meta;
        internal Dictionary<MetaClass, Table> _tablesByMetaClass;
        internal HashSet<MetaEdge> _builtMetaEdges;

        public UniqueNames UniqueNames => _meta._fieldNames;
        public UniqueStrings Strings => _strings;
        public Meta Meta => _meta;
        public Dictionary<MetaClass, Table> TablesByMetaClass => _tablesByMetaClass;
        public HashSet<MetaEdge> BuiltMetaEdges => _builtMetaEdges;

        private Row _emptyRow;
        public Row EmptyRow => _emptyRow ?? (_emptyRow = new Row(this));

        public Repo()
        {
            Reset();
        }

        public void Reset()
        {
            _meta = Meta.G;
            _strings = new UniqueStrings();
            _tablesByMetaClass = new Dictionary<MetaClass, Table>();
            _builtMetaEdges = new HashSet<MetaEdge>();
        }

        public void Add(Table p_Table)
        {
            _tablesByMetaClass.Add(p_Table.MetaClass, p_Table);
        }

        public Table<T> GetTable<T>(bool p_CreateIfNotFound = true)
        {
            return (Table<T>)GetTable((MetaClass)FindOrCreateMetaType(typeof(T)), p_CreateIfNotFound);
        }

        public Table GetTable(Type p_Type, bool p_CreateIfNotFound = true)
        {
            return GetTable((MetaClass)FindOrCreateMetaType(p_Type), p_CreateIfNotFound);
        }

        public Table GetTable(MetaClass p_MetaClass, bool p_CreateIfNotFound = true)
        {
            if (p_MetaClass == null) throw new DalException("Repo.GetTable called with a 'null' MetaClass.");
            if (_tablesByMetaClass.TryGetValue(p_MetaClass, out Table table))
                return table;
            if (p_CreateIfNotFound) {
                return _tablesByMetaClass[p_MetaClass] = p_MetaClass.NewTable(this);
            }
            return null;
        }

        public Table GetLoadedTable(MetaClass p_Type)
        {
            var table = GetTable(p_Type, false);
            if (table != null && table.IsLoaded)
                return table;
            return null;
        }

        public void Add(MetaEdge p_Edge)
        {
            _builtMetaEdges.Add(p_Edge);
        }

        public bool ContainsEdge(MetaEdge p_Edge)
        {
            return _builtMetaEdges.Contains(p_Edge);
        }

        public string GetOrAddString(string p_Str)
        {
            // System.Xml.NameTable is as efficient in both cpu and ram as string.Intern and has the added advantage of being GCeable like anything else.
            // https://stackoverflow.com/questions/29984839/on-string-interning-and-alternatives
            return _strings.Add(p_Str);
        }

        public object Box(object p_Value)
        {
            if (p_Value == null)
                return null;
            if (p_Value == DBNull.Value)
                return null;
            if (p_Value is string)
                return _strings.Add((string)p_Value);
            return p_Value;
        }

        public int ExtractNameIdAndPos(List<int> p_NameIds, string p_Name, out int p_NameId)
        {
            p_NameId = UniqueNames.Find(p_Name);
            if (p_NameId == -1) throw new DalException($"ExtractNameIdAndPos: Unknown name '{p_Name}'.");
            var namePos = p_NameIds.IndexOf(p_NameId);
            if (namePos == -1) throw new DalException($"ExtractNameIdAndPos: Expected name '{p_Name}' name in list.");
            return namePos;
        }

        public List<Exception> ValidateValues()
        {
            var ret = new List<Exception>();
            foreach (var table in _tablesByMetaClass.Values)
                ret.AddRange(table.ValidateValues());
            return ret;
        }

        public List<Exception> ValidateFks()
        {
            var ret = new List<Exception>();
            // Extract most of the pks (missing the ones ending with NOT 'Id', and having more than 1 field.
            var pkFields = (from _t in Meta.MetaTypes.Values
                let t = _t as MetaClass
                where t != null && t.Pk != null && !t.Pk.PkIsDependent
                let pkf = t.Pk.Field
                where pkf.Name.EndsWith("Id")
                select pkf).ToList();
            foreach (var pkField in pkFields)
                ret.AddRange(ValidateFk(pkField));
            return ret;
        }

        public List<Exception> ValidateFk(MetaField pkField)
        {
            var tablePk = GetTable(pkField.MetaClass);
            if (!tablePk.IsLoaded)
                return MetaEdge.EMPTY_EXCEPTIONS;
            var ret = new List<Exception>();
//            Console.WriteLine($"Pk: {tablePk.MetaClass.TypeName}.{tablePk.MetaClass.Pk.Field.Name}");
            foreach (var fkField in pkField.MetaType.RefFields) {
//                Console.WriteLine($"    Fk: {fkField.MetaClass.TypeName}.{fkField.Name}");
                // I don't filter out the pk. It doesn't really matter.
                var tableFk = GetTable(fkField.MetaClass);
                var danglingFks = new List<string>();
                foreach (object fkRow in tableFk) {
                    var fk = (string)fkField.GetFn(fkRow);
                    if (fk == null) continue;
//                    Console.WriteLine($"           Checking: {fk}");
                    if (!tablePk.RowsByPk.TryGetValue(fk, out var _)) {
//                        Console.WriteLine($"            {fk}");
                        danglingFks.Add(fk);
                    }
                }
                if (danglingFks.Count != 0)
                    ret.Add(new DanglingFkException(pkField, fkField, danglingFks));
            }
            return ret;
        }

        public List<Exception> CreateAllPossibleEdges()
        {
            // todo: handle fk with 2 fields (k8s.Pod.OwnerReference.Kind+Name for instance)
            _builtMetaEdges.Clear();
            var ret = new List<Exception>();
            foreach (var edge in _meta.Edges)
                if (edge.AreTablesLoaded(this))
                    ret.AddRange(edge.CreateEdges(this));
            foreach (var table in TablesByMetaClass.Values)
                foreach (var row in table._rowsByPk.Values)
                    table.MetaClass.PostEdges(row);
            return ret;
        }

        public Repo ClearTablesAndEdges()
        {
            foreach (var table in _tablesByMetaClass.Values)
                table.Clear();
            _builtMetaEdges = new HashSet<MetaEdge>();
            return this;
        }

        public Table<T> Load<T>(IDataProxy p_DataProxy, string p_Where = null)
        {
            return (Table<T>)GetTable(typeof(T)).Load(p_DataProxy, p_Where);
        }

        public MetaType FindOrCreateMetaType(Type p_Type)
        {
            return _meta.FindOrCreateMetaType(p_Type);
        }

        public object New(MetaType p_MetaType)
        {
            return p_MetaType.New();
        }

        public void Attach(object p_Row, MetaClass p_MetaClass)
        {
            GetTable(p_MetaClass).Add(p_Row);
        }
        
        public void Set(object p_Row, MetaField p_MetaField, object p_Val)
        {
            p_MetaField.SetFn(p_Row, p_Val is string str ? _strings.Add(str) : p_Val);
        }
        
        public object ParseJson(string p_Str)
        {
            return new JsonParser(this).Parse(p_Str);
        }

        public Row ParseJsonAsRow(string p_Str)
        {
            return (Row)ParseJson(p_Str);
        }

        public dynamic ParseJsonAsDynamic(string p_Str)
        {
            return ParseJson(p_Str);
        }
    }
}