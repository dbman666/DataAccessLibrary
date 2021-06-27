using System;
using System.Collections.Generic;

namespace Coveo.Dal
{
    public class DalException : Exception
    {
        public DalException(string p_Message, Exception p_Inner = null) : base(p_Message, p_Inner)
        {
        }
    }

    public class DanglingFkException : DalException
    {
        public DanglingFkException(MetaField p_PkField, MetaField p_FkField, List<string> p_Fks) : base($"Dangling fks ({p_Fks.Count}) for field '{p_FkField.MetaClass.TypeName}.{p_FkField.Name}' (not found in pk field '{p_PkField.MetaClass.TypeName}.{p_PkField.Name}'): '{string.Join(Ctes.SEP_COMMA_SPACE, p_Fks)}'.")
        {
        }
    }
}