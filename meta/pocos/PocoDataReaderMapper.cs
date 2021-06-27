using System;
using System.Data;

namespace Coveo.Dal
{
    public class PocoDataReaderMapper : PocoMapper
    {
        private IDataReader _dataReader;
        private object[] _values;

        public PocoDataReaderMapper(Table p_Table, IDataReader p_DataReader, bool p_ThrowIfUnknow = false) : base(p_Table, p_ThrowIfUnknow)
        {
            _dataReader = p_DataReader;
            var nb = _dataReader.FieldCount;
            var srcNames = new string[nb];
            var srcTypes = new Type[nb];
            for (int i = 0; i < nb; ++i) {
                srcNames[i] = _dataReader.GetName(i);
                srcTypes[i] = _dataReader.GetFieldType(i);
            }
            Init(srcNames, srcTypes);

            _values = new object[nb];
        }

        public object New()
        {
            _dataReader.GetValues(_values);
            return base.New(_values);
        }
    }
}