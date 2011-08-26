using System;

namespace MySoft.Data
{
    /// <summary>
    /// 表关系类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface ITableRelation<T> where T : Entity
    {
        TableRelation<T> GroupBy(GroupByClip groupBy);
        TableRelation<T> InnerJoin<TJoin>(Table table, WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> InnerJoin<TJoin>(WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> InnerJoin<TJoin>(string aliasName, WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> LeftJoin<TJoin>(Table table, WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> LeftJoin<TJoin>(WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> LeftJoin<TJoin>(string aliasName, WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> OrderBy(OrderByClip orderBy);
        TableRelation<T> RightJoin<TJoin>(Table table, WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> RightJoin<TJoin>(WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> RightJoin<TJoin>(string aliasName, WhereClip onWhere) where TJoin : Entity;
        TableRelation<T> Select(params Field[] fields);
        TableRelation<T> SubQuery();
        TableRelation<T> SubQuery(string aliasName);
        TableRelation<TSub> SubQuery<TSub>() where TSub : Entity;
        TableRelation<TSub> SubQuery<TSub>(string aliasName) where TSub : Entity;
        TableRelation<T> Where(WhereClip where);
    }
}
