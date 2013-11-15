using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    interface IWhereCreator<TCreator>
        where TCreator : class
    {
        TCreator AddWhere(Field field, object value);
        TCreator AddWhere(string fieldName, object value);
        TCreator AddWhere(WhereClip where);
        TCreator AddWhere(string where, params SQLParameter[] parameters);
    }
}
