using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Coveo.Dal
{
    public abstract class Table : IEnumerable
    {
        internal Dictionary<string, object> _rowsByPk;
        private string _sql;

        public Repo Repo { get; }
        public MetaClass MetaClass { get; }
        public Table Parent { get; }
        //public IList Rows { get; }
        public Dictionary<string, object> RowsByPk => _rowsByPk;
        public bool IsLoaded { get; private set; }
        public bool IsEmpty => _rowsByPk.Count == 0;

        public Table(Repo p_Repo, Type p_Type)
        {
            MetaClass = (MetaClass)p_Repo.Meta.FindOrCreateMetaType(p_Type);
            //if (MetaClass.Pk == null) throw new DalException($"Table.ctor: Missing pk for row of type '{p_Type.Name}'.");
            Repo = p_Repo;
            if (MetaClass.ParentType is MetaClass parentClass && parentClass.TableAttribute != null)
                Parent = Repo.GetTable(parentClass);
            _rowsByPk = new Dictionary<string, object>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rowsByPk.Values.GetEnumerator();
        }

        public void Add(object p_Row)
        {
            // Central point to call PostLoad.
            // Repo.Attach redirects here (because it groups by 'table', i.e. 'pk').
            
            MetaClass.PostLoad(p_Row);
            var pk = MetaClass.BuildPk(p_Row);
            _rowsByPk.Add(pk, p_Row);
        }

        public void LoadingIsDone()
        {
            if (MetaClass._postLoadTableMethodInfo != null)
                MetaClass._postLoadTableMethodInfo.Invoke(null, new object[] {Repo});
            IsLoaded = true;
            if (Parent != null)
                Parent.IsLoaded = true;
        }

        public IList RowsAsTypedList()
        {
            var list = MetaClass.NewList();
            foreach (var row in _rowsByPk.Values)
                list.Add(row);
            return list;
        }

        public Table Load(IDataProxy p_DataProxy, string p_Where = null)
        {
            // Names have to be explicitly enumerated because some ones are skipped.
            if (_sql == null) {
                if (MetaClass.TableAttribute.UseSelectStar)
                    _sql = "select * from " + MetaClass.TableAttribute.Name;
                else {
                    var sb = new StringBuilder("select ");
                    int i = 0;
                    foreach (var fld in MetaClass.Fields) {
                        if ((fld._flags & (FieldFlags.IsEdge | FieldFlags.IsComputed | FieldFlags.IsNotUseful)) != 0) continue;
                        if (i++ > 0) sb.CommaSpace();
                        sb.Append(fld.Name);
                    }
                    sb.Append(" from ").Append(MetaClass.TableAttribute.Name);
                    _sql = sb.ToString();
                }
            }
            p_DataProxy.ExecuteQuery(p_Where == null ? _sql : _sql + ' ' + p_Where, this);
            LoadingIsDone();
            return this;
        }

        public List<Exception> ValidateValues()
        {
            var ret = new List<Exception>();
            foreach (var field in MetaClass.Fields) {
                if (field is MetaEdgeField) continue;
                var fieldType = field.MetaType;
                foreach (object row in _rowsByPk.Values) {
                    var val = field.GetFn(row);
                    if (val == null) continue;
                    if (!fieldType.IsValid(val))
                        ret.Add(new DalException($"Invalid value: Field: '{MetaClass.TypeName}.{field.Name}', pk: '{MetaClass.BuildPk(row)}', value: '{val}'."));
                }
            }
            return ret;
        }

        public void Clear()
        {
            _rowsByPk.Clear();
        }

        public Table CloneWithoutData()
        {
            return MetaClass.NewTable(Repo);
        }
    }

    public class Table<T> : Table, IEnumerable<T>
    {
        public Table(Repo p_Repo) : base(p_Repo, typeof(T))
        {
        }

        public void AddRange(List<T> p_Rows)
        {
            foreach (var row in p_Rows) {
                Add(row);
            }
        }

        public new Table<T> Load(IDataProxy p_DataProxy, string p_Where = null)
        {
            base.Load(p_DataProxy, p_Where);
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new TableEnumerator<T>(_rowsByPk.Values.GetEnumerator());
        }

        public bool GetByPk(string pk, out T row)
        {
            if (_rowsByPk.TryGetValue(pk, out var _row)) {
                row = (T)_row;
                return true;
            }
            row = default(T);
            return false;
        }
    }

    public class TableEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator _enumerator;

        public TableEnumerator(IEnumerator p_Enumerator)
        {
            _enumerator = p_Enumerator;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public T Current => (T)_enumerator.Current;

        object IEnumerator.Current => Current;
    }
}