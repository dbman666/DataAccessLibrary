using System;
using System.Collections.Generic;

namespace Coveo.Dal
{
    public interface IDataProxy : IDisposable
    {
        object Execute(string p_Command);

        List<object> ExecuteQuery(string p_Command, Table p_Table); // Stats maybe, since the data will be in the repo; maybe the list, with AUD ?
    }
}
