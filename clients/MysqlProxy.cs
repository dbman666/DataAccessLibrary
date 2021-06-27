using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace Coveo.Dal
{
    public class MysqlProxy : SqlProxy
    {
        public MysqlProxy(string p_UrlParam, string p_UsrParam, string p_PwdParam) : base(p_UrlParam, p_UsrParam, p_PwdParam)
        {
            _connStr = $"server={_urlParam};user={_usrParam};port=3306;password={_pwdParam};compress=true;";
        }

        protected override IDbConnection NewConnection()
        {
            return new MySqlConnection(_connStr);
        }

        public override object Execute(string p_Command)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object[]> RawQuery(string p_Sql, bool p_WithHeaders = false)
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = p_Sql;
            using IDataReader reader = cmd.ExecuteReader();
            // Fields.
            var nbFields = reader.FieldCount;

            if (p_WithHeaders) {
                var nb = reader.FieldCount;
                var colNames = new object[nb];
                for (var i = 0; i < nb; ++i)
                    colNames[i] = reader.GetName(i);
                yield return colNames;
            }

            // Rows.
            while (reader.Read()) {
                var rawValues = new object[nbFields];
                reader.GetValues(rawValues);
                yield return rawValues;
            }
        }

        public StringBuilder RawQueryToJson(StringBuilder sb, string p_Sql, string p_ColumnsField, string p_DataField)
        {
            bool wrap = sb == null;
            if (wrap)
                sb = new StringBuilder("{");
            
            using (var cmd = Connection.CreateCommand()) {
                cmd.CommandText = p_Sql;
                using (IDataReader reader = cmd.ExecuteReader()) {
                    // Fields.
                    var nbFields = reader.FieldCount;

                    if (p_ColumnsField != null) {
                        sb.Append($"\"{p_ColumnsField}\": [");
                        var nb = reader.FieldCount;
                        for (var i = 0; i < nb; ++i)
                            sb.CommaSpace(i).Append($"\"{reader.GetName(i)}\"");
                        sb.Append("],");
                    }

                    // Rows.
                    sb.Append($"\"{p_DataField}\": [");
                    for (var irow = 0; reader.Read(); ++irow) {
                        var rawValues = new object[nbFields];
                        reader.GetValues(rawValues);
                        if (irow != 0)
                            sb.Append(",");
                        sb.Append("[");
                        var icol = 0;
                        foreach (var o in rawValues) {
                            if (icol++ != 0)
                                sb.Append(',');
                            JsonWriter.AppendJson(sb, o);
                        }
                        sb.Append("]");
                    }
                    sb.Append("]");
                }
            }
            if (wrap)
                sb.Append('}');
            return sb;
        }

        public IEnumerable<T> RawQuery<T>(Table<T> p_Table, string p_Sql, bool p_ThrowIfUnknown = true)
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = p_Sql;
            using IDataReader reader = cmd.ExecuteReader();
            var mapper = new PocoDataReaderMapper(p_Table, reader, p_ThrowIfUnknown);
            while (reader.Read()) {
                yield return (T)mapper.New();
            }
        }

        public override List<object> ExecuteQuery(string p_Command, Table p_Table)
        {
            var list = new List<object>();
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = p_Command;
            //Console.WriteLine(p_Command);
            using IDataReader reader = cmd.ExecuteReader();
            var mapper = new PocoDataReaderMapper(p_Table, reader, true);
            while (reader.Read()) {
                list.Add(mapper.New());
            }
            return list;
        }

        public int ExecuteRaw(string p_Command)
        {
            using var cmd = Connection.CreateCommand();
            cmd.CommandText = p_Command;
            return cmd.ExecuteNonQuery();
        }
    }
}