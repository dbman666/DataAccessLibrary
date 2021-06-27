using System;
using System.Collections;
using System.Text;

namespace Coveo.Dal
{
    public class JsonWriterOptions
    {
        public bool SortRowFields;
        public bool Recurse;
        public bool Pretty;
    }

    public class JsonWriter
    {
        private static JsonWriterOptions DefaultOptions = new JsonWriterOptions();

        private StringBuilder _sb;
        private JsonWriterOptions _options;

        public static string ToJson(object val, StringBuilder sb = null, JsonWriterOptions options = null)
        {
            var writer = new JsonWriter(sb, options);
            writer.AppendJson(val, 0);
            return writer._sb.ToString();
        }

        public JsonWriter(StringBuilder sb = null, JsonWriterOptions options = null)
        {
            _sb = sb ?? new StringBuilder();
            _options = options ?? DefaultOptions;
        }

        public void AppendJson(object val, int level)
        {
            if (val == null) {
                _sb.Append(Ctes.NULL);
                return;
            }
            var tc = val is IConvertible ic ? ic.GetTypeCode() : TypeCode.Object;
            switch (tc) {
            case TypeCode.Boolean:
                _sb.Append((bool)val ? Ctes.TRUE : Ctes.FALSE);
                return;
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
                _sb.Append(val);
                return;
            case TypeCode.Int32:
                if (val.GetType().BaseType == typeof(Enum))
                    _sb.Append(Ctes.CHAR_DOUBLE_QUOTE).Append(val).Append(Ctes.CHAR_DOUBLE_QUOTE);
                else
                    _sb.Append(val);
                return;
            case TypeCode.DateTime: // Quoted instead of epoch (milli-)seconds
                _sb.Append(Ctes.CHAR_DOUBLE_QUOTE).Append(val).Append(Ctes.CHAR_DOUBLE_QUOTE);
                return;
            case TypeCode.String:
                var str = (string)val;
                str = str.IndexOf('\\') != -1 ? str.Replace("\\", "\\\\") : str;
                str = str.IndexOf('"') != -1 ? str.Replace("\"", "\\\"") : str;
                _sb.Append(Ctes.CHAR_DOUBLE_QUOTE).Append(str).Append(Ctes.CHAR_DOUBLE_QUOTE);
                return;
            case TypeCode.Object:
                var pretty = _options.Pretty;
                if (val is Row row) {
                    _sb.Append(Ctes.CHAR_OPEN_BRACE);
                    var uniqueNames = row._repo.UniqueNames;
                    var fieldNameIds = row.FieldNameIds;
                    var recurse = _options.Recurse;
                    var sorted = _options.SortRowFields ? row.SortFields() : null;
                    var values = row._values;
                    var nbOut = 0;
                    for (int i = 0; i < fieldNameIds.Count; ++i) {
                        val = sorted == null ? values[i] : values[sorted[i]];
                        if (val == null) continue;
                        if (!recurse && (val is Row || val is IList)) continue;
                        if (nbOut != 0) _sb.CommaSpace();
                        var fieldNameId = sorted == null ? fieldNameIds[i] : fieldNameIds[sorted[i]];
                        _sb.AppendFormat("\"{0}\": ", uniqueNames[fieldNameId]);
                        AppendJson(val, level + 1);
                        if (pretty) _sb.AppendLine();
                        ++nbOut;
                    }
                    _sb.Append(Ctes.CHAR_CLOSE_BRACE);
                    return;
                }
                if (val is IDictionary dict) {
                    _sb.Append(Ctes.CHAR_OPEN_BRACE);
                    var itr = dict.GetEnumerator();
                    var i = 0;
                    while (itr.MoveNext())
                    {
                        if (i++ != 0) _sb.CommaSpace();
                        _sb.AppendFormat("\"{0}\": ", itr.Key);
                        AppendJson(itr.Value, level + 1);
                        if (pretty) _sb.AppendLine();
                    }
                    _sb.Append(Ctes.CHAR_CLOSE_BRACE);
                    return;
                }
                if (val is IEnumerable list) {
                    _sb.Append(Ctes.CHAR_OPEN_SQUARE);
                    var i = 0;
                    foreach (var obj in list) {
                        if (i++ != 0) _sb.CommaSpace();
                        AppendJson(obj, level + 1);
                        if (pretty) _sb.AppendLine();
                    }
                    _sb.Append(Ctes.CHAR_CLOSE_SQUARE);
                    return;
                }
                {
                    var type = val.GetType();
                    _sb.Append(Ctes.CHAR_OPEN_BRACE);
                    int i = 0;
                    foreach (var prop in type.GetProperties()) {
                        var propVal = prop.GetValue(val);
                        if (propVal == null) continue;
                        if (i++ != 0) _sb.CommaSpace();
                        _sb.AppendFormat("\"{0}\": ", prop.Name);
                        AppendJson(propVal, level + 1);
                        if (pretty) _sb.AppendLine();
                    }
                    _sb.Append(Ctes.CHAR_CLOSE_BRACE);
                    return;
                }
                //break;
            case TypeCode.DBNull:
                _sb.Append(Ctes.NULL);
                return;
            }
            throw new DalException($"JsonWriter.AppendJson: Unexpected {val.GetType().Name}.");
        }
        
        public static void AppendJson(StringBuilder sb, object val)
        {
            if (val == null) {
                sb.Append(Ctes.NULL);
                return;
            }
            var tc = val is IConvertible ic ? ic.GetTypeCode() : TypeCode.Object;
            switch (tc) {
            case TypeCode.Boolean:
                sb.Append((bool)val ? Ctes.TRUE : Ctes.FALSE);
                return;
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
                sb.Append(val);
                return;
            case TypeCode.Int32:
                if (val.GetType().BaseType == typeof(Enum))
                    sb.Append(Ctes.CHAR_DOUBLE_QUOTE).Append(val).Append(Ctes.CHAR_DOUBLE_QUOTE);
                else
                    sb.Append(val);
                return;
            case TypeCode.DateTime: // Quoted instead of epoch (milli-)seconds
                sb.Append(Ctes.CHAR_DOUBLE_QUOTE).Append(val).Append(Ctes.CHAR_DOUBLE_QUOTE);
                return;
            case TypeCode.String:
                var str = (string)val;
                str = str.IndexOf('\\') != -1 ? str.Replace("\\", "\\\\") : str;
                str = str.IndexOf('"') != -1 ? str.Replace("\"", "\\\"") : str;
                sb.Append(Ctes.CHAR_DOUBLE_QUOTE).Append(str).Append(Ctes.CHAR_DOUBLE_QUOTE);
                return;
            case TypeCode.Object:
                throw new NotImplementedException();
            case TypeCode.DBNull:
                sb.Append(Ctes.NULL);
                return;
            }
            throw new DalException($"JsonWriter.AppendJson: Unexpected {val.GetType().Name}.");
        }
     }
}