
namespace MySoft.Data
{
    interface IQueryCreator : IWhereCreator<QueryCreator>
    {
        QueryCreator AddField(string tableName, string fieldName);
        QueryCreator AddField(Field field);
        QueryCreator AddField(string fieldName);
        QueryCreator AddOrder(Field field, bool desc);
        QueryCreator AddOrder(string orderby);
        QueryCreator AddOrder(OrderByClip order);
        QueryCreator AddOrder(string fieldName, bool desc);
        QueryCreator Join(Table table, WhereClip where);
        QueryCreator Join(string tableName, string aliasName, string where, params SQLParameter[] parameters);
        QueryCreator Join(string tableName, string where, params SQLParameter[] parameters);
        QueryCreator Join(JoinType joinType, Table table, WhereClip where);
        QueryCreator Join(JoinType joinType, string tableName, string where, params SQLParameter[] parameters);
        QueryCreator Join(JoinType joinType, string tableName, string aliasName, string where, params SQLParameter[] parameters);
        QueryCreator RemoveField(params string[] fieldNames);
        QueryCreator RemoveField(params Field[] fields);
    }
}
