using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    interface ITableCreator<TCreator>
        where TCreator : class
    {
        TCreator From(Table table);
        TCreator From(string tableName);
    }

    interface IWhereCreator<TCreator> : ITableCreator<TCreator>
        where TCreator : class
    {
        TCreator AddWhere(Field field, object value);
        TCreator AddWhere(string fieldName, object value);
        TCreator AddWhere(WhereClip where);
        TCreator AddWhere(string where, params SQLParameter[] parameters);
    }
}
