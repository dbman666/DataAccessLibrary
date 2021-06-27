using System;

namespace Coveo.Dal
{
    public class PocoMapper : IConvContext
    {
        private Table _table;
        private Repo _repo;
        private MetaClass _metaClass;
        private bool _throwIfUnknown;
        private PocoMapperField[] _mapperFields;
        private object _convContext;

        public PocoMapper(Table p_Table, bool p_ThrowIfUnknown = false)
        {
            _table = p_Table;
            _repo = _table.Repo;
            _metaClass = _table.MetaClass;
            _throwIfUnknown = p_ThrowIfUnknown;
        }

        public PocoMapper(Table p_Table, string[] p_SrcNames, Type[] p_SrcTypes, bool p_ThrowIfUnknown = false)
            : this(p_Table, p_ThrowIfUnknown)
        {
            Init(p_SrcNames, p_SrcTypes);
        }

        public void Init(string[] p_SrcNames, Type[] p_SrcTypes)
        {
            var meta = _metaClass.Meta;
            var nameIdxs = _metaClass.FieldNameIdxs;
            var metaFields = _metaClass.Fields;
            var nb = p_SrcNames.Length;
            _mapperFields = new PocoMapperField[nb];
            for (int i = 0; i < nb; ++i) {
                var srcName = p_SrcNames[i];
                var nameIdx = meta.FindField(srcName);
                var idxInPoco = Array.IndexOf(nameIdxs, nameIdx);
                if (idxInPoco == -1) {
                    //Console.WriteLine($"Field '{srcName}' not found in Poco '{_metaType.ClrType.Name}'.");
                    if (_throwIfUnknown) throw new DalException($"Field '{srcName}' not found in Poco '{_metaClass.ClrType.Name}'.");
                    continue;
                }
                var metaField = metaFields[idxInPoco];
                var srcType = p_SrcTypes?[i];
                var mapperField = new PocoMapperField
                {
                    SrcName = srcName,
                    SrcType = srcType,
                    IdxInPoco = idxInPoco,
                    MetaField = metaField
                };
                _mapperFields[i] = mapperField;
                if (metaField.ClrType != srcType)
                    mapperField.ConvFn = TypeConverter.FindConvFn(metaField.ClrType, srcType);
            }
        }

        public object New(object[] p_Values)
        {
            var nb = p_Values.Length;
            if (_mapperFields.Length != nb) throw new DalException($"PocoMapper.New: Inconsistent lengths: Expected {_mapperFields.Length} values, but got {nb}.");
            var obj = _metaClass.New();
            for (var i = 0; i < nb; ++i) {
                var mapperField = _mapperFields[i];
                if (mapperField == null) continue;
                var val = p_Values[i];
                if (val == null || val == DBNull.Value) continue;
                if (mapperField.ConvFn != null)
                    val = mapperField.ConvFn(val, mapperField.MetaField.ClrType, this);
                _repo.Set(obj, mapperField.MetaField, val);
            }
            _table.Add(obj);
            return obj;
        }

        public Repo Repo => _repo;

        public object Context
        {
            get => _convContext;
            set => _convContext = value;
        }
    }
}