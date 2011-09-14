using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using MySoft.Data.Design;
using MySoft.Cache;

namespace MySoft.Data
{
    public class QuerySection<T> : IQuerySection<T>, IPaging
        where T : Entity
    {
        private string formatString = " SELECT {0} {1} {2} {3} FROM {4} {5} {6} {7} {8} {9}";
        private string distinctString;
        private string prefixString;
        private string suffixString;
        private string endString;
        private string sqlString;
        private string queryString;
        private string countString;
        private bool fieldSelect;
        private bool unionQuery;
        private DbProvider dbProvider;
        private Field pagingField;
        private List<Field> fieldList = new List<Field>();
        private List<SQLParameter> parameterList = new List<SQLParameter>();
        private FromSection<T> fromSection;
        private WhereClip havingWhere;
        private WhereClip pageWhere;
        private WhereClip queryWhere;
        private GroupByClip groupBy;
        private OrderByClip orderBy;
        private DbCommand queryCommand;
        private DbTrans dbTran;
        private bool isAddParameter;

        #region 内部成员

        private string countWhereString
        {
            get
            {
                if (DataHelper.IsNullOrEmpty(queryWhere))
                {
                    return null;
                }
                return " WHERE " + queryWhere.ToString();
            }
        }

        private string whereString
        {
            get
            {
                WhereClip where = queryWhere && pageWhere;
                if (DataHelper.IsNullOrEmpty(where))
                {
                    return null;
                }
                return " WHERE " + where.ToString();
            }
        }

        private string groupString
        {
            get
            {
                if (DataHelper.IsNullOrEmpty(groupBy))
                {
                    return null;
                }
                return " GROUP BY " + groupBy.ToString();
            }
        }

        private string havingString
        {
            get
            {
                if (DataHelper.IsNullOrEmpty(havingWhere))
                {
                    return null;
                }
                return " HAVING " + havingWhere.ToString();
            }
        }

        private string CountString
        {
            get
            {
                string sql = null;
                if (countString != null)
                {
                    sql = countString;
                }
                else
                {
                    if (DataHelper.IsNullOrEmpty(groupBy) && distinctString == null)
                    {
                        sql = string.Format(formatString, null, null, "COUNT(*) AS ROW_COUNT",
                            null, SqlString, countWhereString, null, null, null, null);
                    }
                    else
                    {
                        sql = string.Format(formatString, distinctString, null, fieldString,
                           null, SqlString, countWhereString, groupString, havingString, null, endString);
                        sql = string.Format("SELECT COUNT(*) AS ROW_COUNT FROM ({0}) TMP_TABLE", sql);
                    }
                }
                return sql;
            }
            set
            {
                countString = value;
            }
        }

        private string fieldString
        {
            get
            {
                if (fieldList.Count == 0)
                {
                    fieldList.AddRange(fromSection.GetSelectFields());
                }

                StringBuilder sb = new StringBuilder();
                int index = 0;
                foreach (Field field in fieldList)
                {
                    index++;
                    if (field is IProvider)
                    {
                        (field as IProvider).SetDbProvider(dbProvider, dbTran);
                    }
                    sb.Append(field.FullName);
                    if (index < fieldList.Count)
                    {
                        sb.Append(",");
                    }
                }

                return sb.ToString();
            }
        }

        private string SqlString
        {
            get
            {
                if (sqlString == null)
                {
                    sqlString = fromSection.TableName + " " + fromSection.Relation;
                }
                return sqlString;
            }
            set
            {
                sqlString = value;
            }
        }

        #endregion

        #region 获取公有变量

        internal SQLParameter[] Parameters
        {
            get
            {
                if (!isAddParameter)
                {
                    WhereClip where = queryWhere && pageWhere && havingWhere;

                    //给parameterList赋值
                    foreach (SQLParameter p in where.Parameters)
                    {
                        if (!parameterList.Exists(p1 => { return p.Name == p1.Name; }))
                        {
                            parameterList.Add(p);
                        }
                    }

                    isAddParameter = true;
                }

                return parameterList.ToArray();
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > 0)
                    {
                        //给parameterList赋值
                        foreach (SQLParameter p in value)
                        {
                            if (!parameterList.Exists(p1 => { return p.Name == p1.Name; }))
                            {
                                parameterList.Add(p);
                            }
                        }
                    }
                }
            }
        }

        #region fromSection属性

        /// <summary>
        /// 表名
        /// </summary>
        internal FromSection<T> FromSection
        {
            get
            {
                return fromSection;
            }
        }

        internal void SetDbProvider(DbProvider dbProvider, DbTrans dbTran)
        {
            this.dbProvider = dbProvider;
            this.dbTran = dbTran;
        }

        #endregion

        /// <summary>
        /// 前置值如 top n
        /// </summary>
        void IPaging.Prefix(string prefix)
        {
            this.prefixString = prefix;
        }

        /// <summary>
        /// 结尾值如 row_number()
        /// </summary>
        void IPaging.Suffix(string suffix)
        {
            this.suffixString = suffix;
        }

        /// <summary>
        /// 设置值如 limit n
        /// </summary>
        /// <param name="end"></param>
        void IPaging.End(string end)
        {
            this.endString = end;
        }

        internal string QueryString
        {
            get
            {
                if (queryString == null)
                {
                    return string.Format(formatString, distinctString, prefixString, fieldString,
                         suffixString, SqlString, whereString, groupString, havingString, OrderString, endString);
                }
                else
                {
                    if (prefixString != null)
                    {
                        string sql = "(" + queryString + ") " + this.fromSection.TableName;
                        return string.Format(formatString, distinctString, prefixString, fieldString,
                               suffixString, sql, whereString, groupString, havingString, OrderString, endString);
                    }
                    return queryString;
                }
            }
            set
            {
                queryString = value;
            }
        }

        internal string OrderString
        {
            get
            {
                if (DataHelper.IsNullOrEmpty(orderBy))
                {
                    return null;
                }
                return " ORDER BY " + orderBy.ToString();
            }
        }

        internal bool UnionQuery
        {
            get
            {
                return unionQuery;
            }
        }

        internal WhereClip PageWhere
        {
            set
            {
                this.isAddParameter = false;
                pageWhere = value;
            }
        }

        internal Field PagingField
        {
            get
            {
                return pagingField;
            }
            set
            {
                pagingField = value;
            }
        }

        #endregion

        #region QuerySection 初始化

        internal QuerySection(FromSection<T> fromSection, DbProvider dbProvider, DbTrans dbTran, Field pagingField)
        {
            this.fromSection = fromSection;
            this.dbProvider = dbProvider;
            this.dbTran = dbTran;
            this.pagingField = pagingField;
        }

        internal QuerySection(FromSection<T> fromSection)
        {
            this.fromSection = fromSection;
        }

        #endregion

        #region 实现IQuerySection

        #region 实现IDataQuery

        /// <summary>
        /// 返回一个列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public SourceList<TResult> ToList<TResult>(int startIndex, int endIndex)
            where TResult : class
        {
            return ToReader(startIndex, endIndex).ConvertTo<TResult>();
        }

        /// <summary>
        /// 返回一个列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public virtual SourceList<TResult> ToList<TResult>()
            where TResult : class
        {
            return ToReader().ConvertTo<TResult>();
        }

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToSingle<TResult>()
            where TResult : class
        {
            var reader = GetDataReader(this, 1, 0);
            var list = reader.ConvertTo<TResult>();
            if (list.Count > 0)
                return list[0];
            else
                return default(TResult);
        }

        #endregion

        #region 创建子查询

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<T> SubQuery()
        {
            return SubQuery<T>();
        }

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<T> SubQuery(string aliasName)
        {
            return SubQuery<T>(aliasName);
        }

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<TSub> SubQuery<TSub>()
            where TSub : Entity
        {
            return SubQuery<TSub>(null);
        }

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<TSub> SubQuery<TSub>(string aliasName)
            where TSub : Entity
        {
            TSub entity = CoreHelper.CreateInstance<TSub>();
            string tableName = entity.GetTable().Name;
            QuerySection<TSub> query = new QuerySection<TSub>(new FromSection<TSub>(tableName, aliasName), dbProvider, dbTran, pagingField);
            query.SqlString = "(" + QueryString + ") " + (aliasName != null ? "__[" + aliasName + "]__" : tableName);
            query.Parameters = this.Parameters;

            if ((IField)this.pagingField == null)
                query.pagingField = entity.PagingField;

            List<Field> flist = new List<Field>();
            if (aliasName != null)
            {
                if ((IField)query.pagingField != null)
                {
                    query.pagingField = query.pagingField.At(aliasName);
                }

                fieldList.ForEach(field =>
                {
                    flist.Add(field.At(aliasName));
                });
            }
            else
            {
                if ((IField)query.pagingField != null)
                {
                    query.pagingField = query.pagingField.At(tableName);
                }

                fieldList.ForEach(field =>
                {
                    flist.Add(field.At(tableName));
                });
            }

            //如果当前子查询与上一级子查询类型相同，则直接使用上一级的字段列表
            if (typeof(TSub) == typeof(T))
            {
                query.Select(flist.ToArray());
            }

            return query;
        }

        /// <summary>
        /// 创建一个跟当前相同的子查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        internal QuerySection<TResult> CreateQuery<TResult>()
            where TResult : Entity
        {
            QuerySection<TResult> query = new QuerySection<TResult>(new FromSection<TResult>(fromSection.TableName, fromSection.Relation, fromSection.EntityList), dbProvider, dbTran, pagingField);
            query.Where(queryWhere).OrderBy(orderBy).GroupBy(groupBy).Having(havingWhere);
            query.Parameters = this.Parameters;

            if (fieldSelect) query.Select(fieldList.ToArray());
            return query;
        }

        #endregion

        #region 排序分组操作

        /// <summary>
        /// 进行GroupBy操作
        /// </summary>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        public QuerySection<T> GroupBy(GroupByClip groupBy)
        {
            this.groupBy = groupBy;
            return this;
        }

        /// <summary>
        /// 进行OrderBy操作
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public QuerySection<T> OrderBy(OrderByClip orderBy)
        {
            this.orderBy = orderBy;
            return this;
        }

        /// <summary>
        /// 选取前N条数据
        /// </summary>
        /// <param name="topSize"></param>
        /// <returns></returns>
        public TopSection<T> GetTop(int topSize)
        {
            if (topSize <= 0) throw new DataException("选取前N条数据值不能小于等于0！");

            String topString = dbProvider.CreatePageQuery<T>(this, topSize, 0).QueryString;
            TopSection<T> top = new TopSection<T>(topString, fromSection, dbProvider, dbTran, pagingField, topSize);
            top.Where(queryWhere).OrderBy(orderBy).GroupBy(groupBy).Having(havingWhere);
            top.Parameters = this.Parameters;

            if (fieldSelect) top.Select(fieldList.ToArray());
            return top;
        }

        /// <summary>
        /// 设置分页字段
        /// </summary>
        /// <param name="pagingField"></param>
        /// <returns></returns>
        public QuerySection<T> SetPagingField(Field pagingField)
        {
            this.pagingField = pagingField;
            return this;
        }

        /// <summary>
        /// 选择输出的列
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public QuerySection<T> Select(params Field[] fields)
        {
            if (fields == null) return this;

            fieldList.Clear();
            if (fields.Length == 0)
            {
                fieldSelect = false;
                return this;
            }
            else
            {
                fieldSelect = true;
                fieldList.AddRange(fields);
                return this;
            }
        }

        /// <summary>
        /// 选择被排除以外的列（用于列多时排除某几列的情况）
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public QuerySection<T> Select(ExcludeField field)
        {
            List<Field> list = new List<Field>(fromSection.GetSelectFields());
            list.RemoveAll(f =>
            {
                if (field.Fields.Contains(f)) return true;
                return false;
            });

            return Select(list.ToArray());
        }

        /// <summary>
        /// 注入当前查询的条件
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public QuerySection<T> Where(WhereClip where)
        {
            this.isAddParameter = false;
            this.queryWhere = where;
            return this;
        }

        #region Union操作

        /// <summary>
        /// 进行Union操作
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QuerySection<T> Union(QuerySection<T> query)
        {
            return Union(query, false);
        }

        /// <summary>
        /// 进行Union操作
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QuerySection<T> UnionAll(QuerySection<T> query)
        {
            return Union(query, true);
        }

        /// <summary>
        /// 进行Union操作
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isUnionAll"></param>
        /// <returns></returns>
        private QuerySection<T> Union(QuerySection<T> query, bool isUnionAll)
        {
            QuerySection<T> q = CreateQuery<T>();
            q.QueryString = this.QueryString;
            q.CountString = this.CountString;
            q.QueryString += " UNION " + (isUnionAll ? " ALL " : "") + query.QueryString;
            q.CountString += " UNION " + (isUnionAll ? " ALL " : "") + query.CountString;
            q.unionQuery = true;

            //将排序进行合并
            OrderByClip order = this.orderBy && query.orderBy;
            q.Parameters = query.Parameters;

            return q.OrderBy(order);
        }

        #endregion

        /// <summary>
        /// 进行Having操作
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public QuerySection<T> Having(WhereClip where)
        {
            this.isAddParameter = false;
            this.havingWhere = where;
            return this;
        }

        /// <summary>
        /// 进行Distinct操作
        /// </summary>
        /// <returns></returns>
        public QuerySection<T> Distinct()
        {
            this.distinctString = " DISTINCT ";
            return this;
        }

        #endregion

        #region 返回数据

        /// <summary>
        /// 返回一个分页处理的Page节
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PageSection<T> GetPage(int pageSize)
        {
            QuerySection<T> query = this;
            if (this.unionQuery)
            {
                query = SubQuery();
                query.OrderBy(this.orderBy);
            }
            return new PageSection<T>(query, pageSize);
        }

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <returns></returns>
        public T ToSingle()
        {
            ISourceList<T> list = GetList<T>(this, 1, 0);
            if (list.Count == 0)
            {
                return default(T);
            }
            else
            {
                return list[0];
            }
        }

        #region 返回object

        /// <summary>
        /// 返回一个Object列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public ArrayList<object> ToListResult(int startIndex, int endIndex)
        {
            if (startIndex <= 0) startIndex = 1;
            int topItem = endIndex - startIndex + 1;
            return GetListResult<object>(this, topItem, endIndex - topItem);
        }

        /// <summary>
        /// 返回一个Object列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public virtual ArrayList<object> ToListResult()
        {
            return ExcuteDataListResult<object>(this, true);
        }

        /// <summary>
        /// 返回一个Object列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public ArrayList<TResult> ToListResult<TResult>(int startIndex, int endIndex)
        {
            if (startIndex <= 0) startIndex = 1;
            int topItem = endIndex - startIndex + 1;
            return GetListResult<TResult>(this, topItem, endIndex - topItem);
        }

        /// <summary>
        /// 返回一个Object列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public virtual ArrayList<TResult> ToListResult<TResult>()
        {
            return ExcuteDataListResult<TResult>(this, true);
        }

        #endregion

        #region 数据查询

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <returns></returns>
        public virtual SourceList<T> ToList()
        {
            return ExcuteDataList<T>(this, true);
        }

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <returns></returns>
        public SourceList<T> ToList(int startIndex, int endIndex)
        {
            if (startIndex <= 0) startIndex = 1;
            int topItem = endIndex - startIndex + 1;
            return GetList<T>(this, topItem, endIndex - topItem);
        }

        #endregion

        /// <summary>
        /// 返回一个DbReader
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public SourceReader ToReader(int startIndex, int endIndex)
        {
            if (startIndex <= 0) startIndex = 1;
            int topItem = endIndex - startIndex + 1;
            return GetDataReader(this, topItem, endIndex - topItem);
        }

        /// <summary>
        /// 返回一个DbReader
        /// </summary>
        /// <returns></returns>
        public virtual SourceReader ToReader()
        {
            return ExcuteDataReader(this, true);
        }

        /// <summary>
        /// 返回一个DataTable
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public SourceTable ToTable(int startIndex, int endIndex)
        {
            if (startIndex <= 0) startIndex = 1;
            int topItem = endIndex - startIndex + 1;
            return GetDataTable(this, topItem, endIndex - topItem);
        }

        /// <summary>
        /// 返回一个DataTable
        /// </summary>
        /// <returns></returns>
        public virtual SourceTable ToTable()
        {
            return ExcuteDataTable(this, true);
        }

        /// <summary>
        /// 返回一个值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToScalar<TResult>()
        {
            object obj = this.ToScalar();
            return CoreHelper.ConvertValue<TResult>(obj);
        }

        /// <summary>
        /// 返回一个值
        /// </summary>
        /// <returns></returns>
        public object ToScalar()
        {
            return ExcuteScalar(this, true);
        }

        /// <summary>
        /// 返回当前查询记录数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return GetCount(this);
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public int GetPageCount(int pageSize)
        {
            return GetPage(pageSize).PageCount;
        }

        #endregion

        #endregion

        #region 公用的方法

        private SourceList<TResult> GetList<TResult>(QuerySection<TResult> query, int itemCount, int skipCount)
            where TResult : Entity
        {
            query = dbProvider.CreatePageQuery<TResult>(query, itemCount, skipCount);
            return ExcuteDataList<TResult>(query, false);
        }

        private ArrayList<TResult> GetListResult<TResult>(QuerySection<T> query, int itemCount, int skipCount)
        {
            query = dbProvider.CreatePageQuery<T>(query, itemCount, skipCount);
            return ExcuteDataListResult<TResult>(query, false);
        }

        private SourceReader GetDataReader(QuerySection<T> query, int itemCount, int skipCount)
        {
            query = dbProvider.CreatePageQuery<T>(query, itemCount, skipCount);
            return ExcuteDataReader(query, false);
        }

        private SourceTable GetDataTable(QuerySection<T> query, int itemCount, int skipCount)
        {
            query = dbProvider.CreatePageQuery<T>(query, itemCount, skipCount);
            return ExcuteDataTable(query, false);
        }

        private int GetCount(QuerySection<T> query)
        {
            string countString = query.CountString;
            if (query.unionQuery)
            {
                countString = "SELECT SUM(ROW_COUNT) AS TOTAL_ROW_COUNT FROM (" + countString + ") TMP_TABLE";
            }

            string cacheKey = GetCacheKey(countString, this.Parameters);
            object obj = GetCache<T>("Count", cacheKey);
            if (obj != null)
            {
                return CoreHelper.ConvertValue<int>(obj);
            }

            //添加参数到Command中
            queryCommand = dbProvider.CreateSqlCommand(countString, query.Parameters);

            object value = dbProvider.ExecuteScalar(queryCommand, dbTran);

            int ret = CoreHelper.ConvertValue<int>(value);

            SetCache<T>("Count", cacheKey, ret);

            return ret;
        }

        #endregion

        #region 私有方法

        private ArrayList<TResult> ExcuteDataListResult<TResult>(QuerySection<T> query, bool all)
        {
            try
            {
                string queryString = query.QueryString;
                if (this.unionQuery && all)
                {
                    queryString += OrderString;
                }

                string cacheKey = GetCacheKey(queryString, this.Parameters);
                object obj = GetCache<T>("ListObject", cacheKey);
                if (obj != null)
                {
                    return (SourceList<TResult>)obj;
                }

                using (SourceReader reader = ExcuteDataReader(query, all))
                {
                    ArrayList<TResult> list = new ArrayList<TResult>();

                    if (typeof(TResult) == typeof(object[]))
                    {
                        while (reader.Read())
                        {
                            List<object> objs = new List<object>();
                            for (int row = 0; row < reader.FieldCount; row++)
                            {
                                objs.Add(reader.GetValue(row));
                            }

                            TResult result = (TResult)(objs.ToArray() as object);
                            list.Add(result);
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetValue<TResult>(0));
                        }
                    }

                    SetCache<T>("ListObject", cacheKey, list);

                    reader.Close();

                    return list;
                }
            }
            catch
            {
                throw;
            }
        }

        private SourceList<TResult> ExcuteDataList<TResult>(QuerySection<TResult> query, bool all)
            where TResult : Entity
        {
            try
            {
                string queryString = query.QueryString;
                if (this.unionQuery && all)
                {
                    queryString += OrderString;
                }

                string cacheKey = GetCacheKey(queryString, this.Parameters);
                object obj = GetCache<TResult>("ListEntity", cacheKey);
                if (obj != null)
                {
                    return (SourceList<TResult>)obj;
                }

                using (SourceReader reader = ExcuteDataReader(query, all))
                {
                    SourceList<TResult> list = new SourceList<TResult>();

                    FastCreateInstanceHandler creator = CoreHelper.GetFastInstanceCreator(typeof(TResult));

                    while (reader.Read())
                    {
                        TResult entity = (TResult)creator();
                        entity.SetDbValues(reader);
                        entity.Attach();
                        list.Add(entity);
                    }

                    SetCache<TResult>("ListEntity", cacheKey, list);

                    reader.Close();

                    return list;
                }
            }
            catch
            {
                throw;
            }
        }

        private SourceReader ExcuteDataReader<TResult>(QuerySection<TResult> query, bool all)
            where TResult : Entity
        {
            try
            {
                if (all)
                {
                    prefixString = null;
                    endString = null;
                }

                string queryString = query.QueryString;
                if (this.unionQuery && all)
                {
                    queryString += OrderString;
                }

                //添加参数到Command中
                queryCommand = dbProvider.CreateSqlCommand(queryString, query.Parameters);

                return dbProvider.ExecuteReader(queryCommand, dbTran);
            }
            catch
            {
                throw;
            }
        }

        private SourceTable ExcuteDataTable<TResult>(QuerySection<TResult> query, bool all)
            where TResult : Entity
        {
            try
            {
                if (all)
                {
                    prefixString = null;
                    endString = null;
                }

                string queryString = query.QueryString;
                if (this.unionQuery && all)
                {
                    queryString += OrderString;
                }

                string cacheKey = GetCacheKey(queryString, this.Parameters);
                object obj = GetCache<TResult>("DataTable", cacheKey);
                if (obj != null)
                {
                    return (SourceTable)obj;
                }

                //添加参数到Command中
                queryCommand = dbProvider.CreateSqlCommand(queryString, query.Parameters);

                using (DataTable dataTable = dbProvider.ExecuteDataTable(queryCommand, dbTran))
                {
                    dataTable.TableName = typeof(TResult).Name;
                    SourceTable table = new SourceTable(dataTable);

                    SetCache<TResult>("DataTable", cacheKey, table);

                    return table;
                }
            }
            catch
            {
                throw;
            }
        }

        private object ExcuteScalar<TResult>(QuerySection<TResult> query, bool all)
            where TResult : Entity
        {
            try
            {
                if (all)
                {
                    prefixString = null;
                    endString = null;
                }

                string queryString = query.QueryString;
                if (this.unionQuery && all)
                {
                    queryString += OrderString;
                }

                string cacheKey = GetCacheKey(queryString, this.Parameters);
                object obj = GetCache<TResult>("GetObject", cacheKey);
                if (obj != null)
                {
                    return obj;
                }

                //添加参数到Command中
                queryCommand = dbProvider.CreateSqlCommand(queryString, query.Parameters);

                object newobj = dbProvider.ExecuteScalar(queryCommand, dbTran);

                SetCache<TResult>("GetObject", cacheKey, newobj);

                return newobj;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region 缓存策略

        /// <summary>
        /// 获取缓存的Key
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        private string GetCacheKey(string sql, SQLParameter[] parameters)
        {
            sql = dbProvider.FormatCommandText(sql);
            if (parameters == null) return sql;
            foreach (var p in parameters)
            {
                sql = sql.Replace(p.Name, DataHelper.FormatValue(p.Value));
            }
            return sql;
        }

        #region 检测是否有缓存

        /// <summary>
        /// 获取缓存信息
        /// </summary>
        /// <typeparam name="CacheType"></typeparam>
        /// <param name="prefix"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private object GetCache<CacheType>(string prefix, string cacheKey)
            where CacheType : Entity
        {
            string key = string.Concat(prefix, "_", cacheKey);
            if (dbProvider.Cache != null)
            {
                return dbProvider.Cache.GetCache(typeof(CacheType), key);
            }
            return null;
        }

        #endregion

        #region 将数据装入缓存

        /// <summary>
        /// 设置缓存信息
        /// </summary>
        /// <typeparam name="CacheType"></typeparam>
        /// <param name="prefix"></param>
        /// <param name="cacheKey"></param>
        /// <param name="obj"></param>
        private void SetCache<CacheType>(string prefix, string cacheKey, object obj)
            where CacheType : Entity
        {
            string key = string.Concat(prefix, "_", cacheKey);
            if (dbProvider.Cache != null)
            {
                int timeout = EntityConfig.Instance.GetTableTimeout<CacheType>();
                dbProvider.Cache.AddCache(typeof(CacheType), key, obj, timeout);
            }
        }

        #endregion

        #endregion

        #region 返回分页信息

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataPage<IList<T>> ToListPage(int pageSize, int pageIndex)
        {
            DataPage<IList<T>> view = new DataPage<IList<T>>(pageSize);
            PageSection<T> page = GetPage(pageSize);
            view.CurrentPageIndex = pageIndex;
            view.RowCount = page.RowCount;
            view.DataSource = page.ToList(pageIndex);
            return view;
        }

        /// <summary>
        /// 返回DataPage
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataPage<DataTable> ToTablePage(int pageSize, int pageIndex)
        {
            DataPage<DataTable> view = new DataPage<DataTable>(pageSize);
            PageSection<T> page = GetPage(pageSize);
            view.CurrentPageIndex = pageIndex;
            view.RowCount = page.RowCount;
            view.DataSource = page.ToTable(pageIndex);
            return view;
        }

        #endregion
    }
}