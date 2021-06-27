using System;
using System.Collections.Generic;
using System.Data;

namespace Coveo.Dal
{
    public abstract class SqlProxy : IDataProxy
    {
        protected static TimeSpan CONN_TTL = TimeSpan.FromHours(8);

        protected string _urlParam { get; }
        protected string _usrParam { get; }
        protected string _pwdParam { get; }
        
        protected string _connStr;
        protected IDbConnection _conn;
        protected DateTime _connTime;

        protected abstract IDbConnection NewConnection();
        public abstract object Execute(string p_Command);
        public abstract List<object> ExecuteQuery(string p_Command, Table p_Table);

        public SqlProxy(string p_UrlParam, string p_UsrParam, string p_PwdParam)
        {
            _urlParam = p_UrlParam;
            _usrParam = p_UsrParam;
            _pwdParam = p_PwdParam;
        }

        public void Dispose()
        {
            CloseConnection();
        }

        public void CloseConnection()
        {
            _conn?.Dispose();
            _conn = null;
        }

        public IDbConnection Connection
        {
            get
            {
                var utcNow = DateTime.UtcNow;
                if (_conn == null || utcNow - _connTime > CONN_TTL) {
                    _conn = NewConnection();
                    _conn.Open();
                    _connTime = utcNow;
                }
                return _conn;
            }
        }
    }
}