using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Coveo.Dal
{
    // The base 'object'.
    // The goal is to be able to store anything (coming from Json, Mysql, etc), so we can later apply validation rules.
    // So we could read a string, and the generated 'typed' getter would fail if expecting an int, or enum value for which the string doesn't map.
    // Plus it gives us the ability to be anonymous/dynamic.
    // I.e. again, read whatever from Json, then apply a linq expression with dot notation and it will work.
    //
    // DynamicObject really spans the whole spectrum of what can be done with an expression. But it uses *Binders to do it.

    // Next: The list of fields should be what is 'unmapped'. So that we'd still be able to have a part that is known, and another that is not.
    //       Beware though, because the name and value can be at the same [i].
    //       Could 'anonymous' types be used ? For tables.
    // So I want to rapidly go from string (field name mostly, maybe class name, proc name, ... ?) to MetaField or something. It's really a term lookup.

    public class Row : DynamicObject
    {
        protected internal Repo _repo;
        private List<int> _fieldNameIds;
        protected internal List<object> _values;

        public Repo Repo => _repo;
        public List<int> FieldNameIds => _fieldNameIds;
        public List<object> Values => _values;
        public RowType RowType;

        public object this[string p_Name]
        {
            get
            {
                var idx = FindField(p_Name);
                return idx == -1 ? null : _values[idx];
            }
            set
            {
                var idx = FindField(p_Name);
                if (idx == -1) {
                    if (value != null)
                        Add(p_Name, value);
                } else {
                    if (value == null) {
                        _fieldNameIds.RemoveAt(idx);
                        _values.RemoveAt(idx);
                    } else {
                        _values[idx] = _repo.Box(value);
                    }
                }
            }
        }

        public object this[int p_FieldId]
        {
            get
            {
                if (p_FieldId == -1 || _fieldNameIds == null) return null;
                var pos = _fieldNameIds.IndexOf(p_FieldId);
                return pos == -1 ? null : _values[pos];
            }
            set
            {
                if (p_FieldId == -1 || _fieldNameIds == null) return;
                var pos = _fieldNameIds.IndexOf(p_FieldId);
                if (pos == -1) return;
                _values[pos] = value;
            }
        }

        public Row()
        {
        }

        public Row(Repo p_Repo)
        {
            _repo = p_Repo;
        }

        public Row(Repo p_Repo, List<int> p_FieldNameIds, object[] p_Values)
        {
            _repo = p_Repo;
            _fieldNameIds = p_FieldNameIds;
            _values = new List<object>(p_Values);
        }

        public void Add(string p_Field, object p_Value)
        {
            if (_fieldNameIds == null) {
                _fieldNameIds = new List<int>();
                _values = new List<object>();
            }
            _fieldNameIds.Add(_repo._meta._fieldNames.Find(p_Field, true));
            _values.Add(_repo.Box(p_Value));
        }

        public Row Clone()
        {
            var ret = new Row(_repo);
            ret._repo = _repo;
            ret._fieldNameIds = new List<int>(_fieldNameIds);
            ret._values = new List<object>(_values);
            return ret;
        }

        public bool Contains(string p_Name)
        {
            return _repo._meta._fieldNames.Contains(p_Name);
        }

        public int FindField(string p_Name)
        {
            if (_fieldNameIds == null) return -1;
            var pos = _repo._meta._fieldNames.Find(p_Name);
            return pos == -1 ? -1 : _fieldNameIds.IndexOf(pos);
        }

        public void Set(int p_PropIdx, object p_Value)
        {
            if (p_Value == null) {
                _values.RemoveAt(p_PropIdx);
            } else {
                _values[p_PropIdx] = p_Value;
            }
        }

        public void RemoveField(string p_Name)
        {
            var pos = _repo._meta._fieldNames.Find(p_Name);
            if (pos != -1) {
                RemoveFieldAt(_fieldNameIds.IndexOf(pos));
            }
        }

        public void RemoveFieldAt(int p_Pos)
        {
            if (p_Pos != -1) {
                _fieldNameIds.RemoveAt(p_Pos);
                _values.RemoveAt(p_Pos);
            }
        }

        public Row Null(string p_FieldName)
        {
            var pos = FindField(p_FieldName);
            if (pos != -1)
                // Incomplete: would have to remove from the FieldNameIds, but that would touch
                _values[pos] = null;
            return this;
        }

        public int[] SortFields()
        {
            var uniqueNames = _repo.UniqueNames;
            var nb = _fieldNameIds.Count;
            var sorted = new int[nb];
            for (int i = 0; i < nb; ++i)
                sorted[i] = i;
            return sorted.OrderBy(n => uniqueNames[_fieldNameIds[n]]).ToArray();
        }

        public Dictionary<string, object> ToDict()
        {
            var ret = new Dictionary<string, object>();
            var uniqueNames = _repo.UniqueNames;
            int nb = _values.Count;
            for (int i = 0; i < nb; ++i) {
                ret[uniqueNames[_fieldNameIds[i]]] = _values[i];
            }
            return ret;
        }

        public IEnumerable<(string, object)> ForAllFields()
        {
            var uniqueNames = _repo.UniqueNames;
            int nb = _values.Count;
            for (int i = 0; i < nb; ++i) {
                yield return (uniqueNames[_fieldNameIds[i]], _values[i]);
            }
        }

        public List<string> FieldNames
        {
            get
            {
                var ret = new List<string>();
                var uniqueNames = _repo.UniqueNames;
                if (_fieldNameIds != null)
                    foreach (var id in _fieldNameIds)
                        ret.Add(uniqueNames[id]);
                return ret;
            }
        }

        public void ForEachNameValue(Action<string, object> p_Fn)
        {
            var uniqueNames = _repo.UniqueNames;
            int nb = _values.Count;
            for (int i = 0; i < nb; ++i) {
                p_Fn(uniqueNames[_fieldNameIds[i]], _values[i]);
            }
        }

        public object ToDump()
        {
            // Don't like doing this at all.
            IDictionary<string, object> expando = new ExpandoObject();
            if (_fieldNameIds != null)
                for (int i = 0; i < _fieldNameIds.Count; ++i) {
                    var val = _values[i];
                    if (val == null) continue;
                    //if (val is Row) val = "<Row>";
                    if (val is string str && str.Length > 30)
                        val = str.Substring(0, 30) + "...";
                    expando.Add(_repo.UniqueNames[FieldNameIds[i]], val);
                }
            return expando;
        }

        // If you try to get a value of a property not defined in the class, this method is called.
        public override bool TryGetMember(GetMemberBinder p_Binder, out object p_Result)
        {
            int idx = FindField(p_Binder.Name);
            p_Result = idx == -1 ? null : _values[idx];
            return true;
        }

        // If you try to set a value of a property that is not defined in the class, this method is called.
        public override bool TrySetMember(SetMemberBinder p_Binder, object p_Value)
        {
            int idx = FindField(p_Binder.Name);
            if (idx == -1) {
                Add(p_Binder.Name, p_Value);
                return true;
            }
            _values[idx] = _repo.Box(p_Value);
            return true;
        }
    }
}