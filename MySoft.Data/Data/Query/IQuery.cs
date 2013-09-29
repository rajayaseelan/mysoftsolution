using System.Collections.Generic;
using System.Data;

namespace MySoft.Data
{
    /// <summary>
    /// 连接类型
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// 左链接
        /// </summary>
        LeftJoin,
        /// <summary>
        /// 右链接
        /// </summary>
        RightJoin,
        /// <summary>
        /// 内部链接
        /// </summary>
        InnerJoin
    }

    public interface IPaging
    {
        /// <summary>
        /// 设置前缀
        /// </summary>
        /// <param name="prefix"></param>
        void Prefix(string prefix);

        /// <summary>
        /// 设置后缀
        /// </summary>
        /// <param name="suffix"></param>
        void Suffix(string suffix);

        /// <summary>
        /// 设置结尾
        /// </summary>
        /// <param name="end"></param>
        void End(string end);
    }

    interface IQuerySection<T> : IQuery<T> where T : Entity
    {
        #region 分组排序

        QuerySection<T> GroupBy(GroupByClip groupBy);
        QuerySection<T> GroupBy(Field[] fields);
        QuerySection<T> Having(WhereClip where);
        QuerySection<T> Select(params Field[] fields);
        QuerySection<T> Select(IFieldFilter filter);
        QuerySection<T> Where(WhereClip where);
        QuerySection<T> Union(QuerySection<T> query);
        QuerySection<T> UnionAll(QuerySection<T> query);

        #endregion

        #region 创建子查询

        QuerySection<T> SubQuery();
        QuerySection<T> SubQuery(string aliasName);
        QuerySection<TSub> SubQuery<TSub>() where TSub : Entity;
        QuerySection<TSub> SubQuery<TSub>(string aliasName) where TSub : Entity;

        #endregion

        #region 返回数据

        ArrayList<object> ToListResult();
        ArrayList<object> ToListResult(int startIndex, int endIndex);

        ArrayList<TResult> ToListResult<TResult>();
        ArrayList<TResult> ToListResult<TResult>(int startIndex, int endIndex);

        SourceReader ToReader();
        SourceReader ToReader(int startIndex, int endIndex);

        TResult ToScalar<TResult>();
        object ToScalar();
        int Count();

        int GetPageCount(int pageSize);

        #endregion

        DataPage<IList<T>> ToListPage(int pageSize, int pageIndex);
        DataPage<IList<TResult>> ToListPage<TResult>(int pageSize, int pageIndex) where TResult : class;
        DataPage<DataTable> ToTablePage(int pageSize, int pageIndex);
        DataPage<DataSet> ToDataSetPage(int pageSize, int pageIndex);
    }

    interface IQuery<T> where T : Entity
    {
        T ToSingle();

        DataSet ToDataSet();
        DataSet ToDataSet(int startIndex, int endIndex);

        SourceTable ToTable();
        SourceTable ToTable(int startIndex, int endIndex);

        SourceList<T> ToList();
        SourceList<T> ToList(int startIndex, int endIndex);

        QuerySection<T> SetPagingField(Field pagingField);
        QuerySection<T> Distinct();
        QuerySection<T> OrderBy(OrderByClip orderBy);
        QuerySection<T> OrderBy(Field[] fields, bool desc);
        QuerySection<T> GetTop(int topSize);
        PageSection<T> GetPage(int pageSize);

        TResult ToSingle<TResult>() where TResult : class;

        SourceList<TResult> ToList<TResult>() where TResult : class;
        SourceList<TResult> ToList<TResult>(int startIndex, int endIndex) where TResult : class;
    }
}
