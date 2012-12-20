using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySoft.Data.Design;

namespace MySoft.Data
{
    /// <summary>
    /// Form处理节
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FromSection<T> : IQuerySection<T>
        where T : Entity
    {
        private QuerySection<T> query;
        /// <summary>
        /// 当前查询对象
        /// </summary>
        internal QuerySection<T> Query
        {
            get { return query; }
            set { query = value; }
        }

        private List<TableEntity> entities = new List<TableEntity>();
        /// <summary>
        /// 当前实例列表
        /// </summary>
        internal List<TableEntity> TableEntities
        {
            get { return entities; }
            set { entities = value; }
        }

        #region 初始化FromSection

        internal FromSection(DbProvider dbProvider, DbTrans dbTran, Table table, string aliasName)
        {
            InitForm(table, aliasName);

            this.query = new QuerySection<T>(this, dbProvider, dbTran);
        }

        internal FromSection(Table table, string aliasName)
        {
            InitForm(table, aliasName);

            this.query = new QuerySection<T>(this);
        }

        internal void InitForm(Table table, string aliasName)
        {
            var entity = CoreHelper.CreateInstance<T>();
            table = table ?? entity.GetTable();

            table.As(aliasName);
            var tableEntity = new TableEntity { Table = table, Entity = entity };
            this.entities.Add(tableEntity);

            SetTableName(table);
        }

        internal void SetTableName(Table table)
        {
            this.tableName = table.FullName;
        }

        #endregion

        #region 实现IQuerySection

        #region 实现IDataQuery

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToSingle<TResult>()
            where TResult : class
        {
            return query.ToSingle<TResult>();
        }

        #endregion

        #region 创建子查询

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<T> SubQuery()
        {
            return query.SubQuery();
        }

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<T> SubQuery(string aliasName)
        {
            return query.SubQuery(aliasName);
        }

        /// <summary>
        /// 生成一个子查询
        /// </summary>
        /// <returns></returns>
        public QuerySection<TSub> SubQuery<TSub>()
            where TSub : Entity
        {
            return query.SubQuery<TSub>();
        }

        /// <summary>
        /// 生成一个带别名的子查询
        /// </summary>
        /// <param name="aliasName"></param>
        /// <returns></returns>
        public QuerySection<TSub> SubQuery<TSub>(string aliasName)
            where TSub : Entity
        {
            return query.SubQuery<TSub>(aliasName);
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
            return query.GroupBy(groupBy);
        }

        /// <summary>
        /// 进行OrderBy操作
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public QuerySection<T> OrderBy(OrderByClip orderBy)
        {
            return query.OrderBy(orderBy);
        }

        #region 通过Field产生对象

        /// <summary>
        /// 进行GroupBy操作
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public QuerySection<T> GroupBy(Field[] fields)
        {
            return query.GroupBy(fields);
        }

        /// <summary>
        /// 进行OrderBy操作
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public QuerySection<T> OrderBy(Field[] fields, bool desc)
        {
            return query.OrderBy(fields, desc);
        }

        #endregion

        /// <summary>
        /// 选择输出的列
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public QuerySection<T> Select(params Field[] fields)
        {
            return query.Select(fields);
        }

        /// <summary>
        /// 选择被排除以外的列（用于列多时排除某几列的情况）
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public QuerySection<T> Select(IFieldFilter filter)
        {
            return query.Select(filter);
        }

        /// <summary>
        /// 注入当前查询的条件
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public QuerySection<T> Where(WhereClip where)
        {
            return query.Where(where);
        }

        /// <summary>
        /// 进行Union操作
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QuerySection<T> Union(QuerySection<T> uquery)
        {
            return query.Union(uquery);
        }

        /// <summary>
        /// 进行Union操作
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QuerySection<T> UnionAll(QuerySection<T> uquery)
        {
            return query.UnionAll(uquery);
        }

        /// <summary>
        /// 进行Having操作
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public QuerySection<T> Having(WhereClip where)
        {
            return query.Having(where);
        }

        /// <summary>
        /// 设置分页字段
        /// </summary>
        /// <param name="pagingField"></param>
        /// <returns></returns>
        public QuerySection<T> SetPagingField(Field pagingField)
        {
            return query.SetPagingField(pagingField);
        }

        /// <summary>
        /// 选择前n条
        /// </summary>
        /// <param name="topSize"></param>
        /// <returns></returns>
        public QuerySection<T> GetTop(int topSize)
        {
            return query.GetTop(topSize);
        }

        /// <summary>
        /// 进行Distinct操作
        /// </summary>
        /// <returns></returns>
        public QuerySection<T> Distinct()
        {
            return query.Distinct();
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
            return query.GetPage(pageSize);
        }

        /// <summary>
        /// 返回一个实体
        /// </summary>
        /// <returns></returns>
        public T ToSingle()
        {
            return query.ToSingle();
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
            return query.ToListResult(startIndex, endIndex);
        }

        /// <summary>
        /// 返回一个Object列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public ArrayList<object> ToListResult()
        {
            return query.ToListResult();
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
            return query.ToListResult<TResult>(startIndex, endIndex);
        }

        /// <summary>
        /// 返回一个Object列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public ArrayList<TResult> ToListResult<TResult>()
        {
            return query.ToListResult<TResult>();
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
            return query.ToReader(startIndex, endIndex);
        }

        /// <summary>
        /// 返回一个DbReader
        /// </summary>
        /// <returns></returns>
        public SourceReader ToReader()
        {
            return query.ToReader();
        }

        #region 数据查询

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <returns></returns>
        public SourceList<T> ToList()
        {
            return query.ToList();
        }

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <returns></returns>
        public SourceList<T> ToList(int startIndex, int endIndex)
        {
            return query.ToList(startIndex, endIndex);
        }

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <returns></returns>
        public SourceList<TResult> ToList<TResult>()
            where TResult : class
        {
            return query.ToList<TResult>();
        }

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <returns></returns>
        public SourceList<TResult> ToList<TResult>(int startIndex, int endIndex)
            where TResult : class
        {
            return query.ToList<TResult>(startIndex, endIndex);
        }

        #endregion

        /// <summary>
        /// 返回一个DataTable
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public SourceTable ToTable(int startIndex, int endIndex)
        {
            return query.ToTable(startIndex, endIndex);
        }

        /// <summary>
        /// 返回一个DataTable
        /// </summary>
        /// <returns></returns>
        public SourceTable ToTable()
        {
            return query.ToTable();
        }

        /// <summary>
        /// 返回一个DataSet
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public DataSet ToDataSet(int startIndex, int endIndex)
        {
            return query.ToDataSet(startIndex, endIndex);
        }

        /// <summary>
        /// 返回一个DataSet
        /// </summary>
        /// <returns></returns>
        public DataSet ToDataSet()
        {
            return query.ToDataSet();
        }

        /// <summary>
        /// 返回一个值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToScalar<TResult>()
        {
            return query.ToScalar<TResult>();
        }

        /// <summary>
        /// 返回一个值
        /// </summary>
        /// <returns></returns>
        public object ToScalar()
        {
            return query.ToScalar();
        }

        /// <summary>
        /// 返回当前查询记录数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return query.Count();
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public int GetPageCount(int pageSize)
        {
            return query.GetPageCount(pageSize);
        }

        #endregion

        #region 返回分页信息

        public DataPage<IList<T>> ToListPage(int pageSize, int pageIndex)
        {
            return query.ToListPage(pageSize, pageIndex);
        }

        /// <summary>
        /// 返回指定数据源的分页信息
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataPage<DataTable> ToTablePage(int pageSize, int pageIndex)
        {
            return query.ToTablePage(pageSize, pageIndex);
        }

        /// <summary>
        /// 返回指定数据源的分页信息
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataPage<DataSet> ToDataSetPage(int pageSize, int pageIndex)
        {
            return query.ToDataSetPage(pageSize, pageIndex);
        }

        #endregion

        #endregion

        #region 连接查询

        #region 内连接

        /// <summary>
        /// 内连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> InnerJoin<TJoin>(WhereClip onWhere)
            where TJoin : Entity
        {
            return InnerJoin<TJoin>((Table)null, onWhere);
        }

        /// <summary>
        /// 内连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="table"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> InnerJoin<TJoin>(Table table, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>(table, null, onWhere, JoinType.InnerJoin);
        }

        /// <summary>
        /// 内连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="aliasName"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> InnerJoin<TJoin>(string aliasName, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>((Table)null, aliasName, onWhere, JoinType.InnerJoin);
        }

        /// <summary>
        /// 内连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="relation"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> InnerJoin<TJoin>(TableRelation<TJoin> relation, WhereClip onWhere)
            where TJoin : Entity
        {
            return InnerJoin(relation, null, onWhere);
        }

        /// <summary>
        /// 内连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="relation"></param>
        /// <param name="aliasName"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> InnerJoin<TJoin>(TableRelation<TJoin> relation, string aliasName, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>(relation, aliasName, onWhere, JoinType.InnerJoin);
        }

        #endregion

        #region 左连接

        /// <summary>
        /// 左连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> LeftJoin<TJoin>(WhereClip onWhere)
            where TJoin : Entity
        {
            return LeftJoin<TJoin>((Table)null, onWhere);
        }

        /// <summary>
        /// 左连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="table"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> LeftJoin<TJoin>(Table table, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>(table, null, onWhere, JoinType.LeftJoin);
        }

        /// <summary>
        /// 左连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="aliasName"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> LeftJoin<TJoin>(string aliasName, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>((Table)null, aliasName, onWhere, JoinType.LeftJoin);
        }

        /// <summary>
        /// 左连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="relation"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> LeftJoin<TJoin>(TableRelation<TJoin> relation, WhereClip onWhere)
            where TJoin : Entity
        {
            return LeftJoin(relation, null, onWhere);
        }

        /// <summary>
        /// 左连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="relation"></param>
        /// <param name="aliasName"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> LeftJoin<TJoin>(TableRelation<TJoin> relation, string aliasName, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>(relation, aliasName, onWhere, JoinType.LeftJoin);
        }

        #endregion

        #region 右连接

        /// <summary>
        /// 右连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> RightJoin<TJoin>(WhereClip onWhere)
            where TJoin : Entity
        {
            return RightJoin<TJoin>((Table)null, onWhere);
        }

        /// <summary>
        /// 右连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="table"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> RightJoin<TJoin>(Table table, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>(table, null, onWhere, JoinType.RightJoin);
        }

        /// <summary>
        /// 右连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="aliasName"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> RightJoin<TJoin>(string aliasName, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>((Table)null, aliasName, onWhere, JoinType.RightJoin);
        }

        /// <summary>
        /// 右连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="relation"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> RightJoin<TJoin>(TableRelation<TJoin> relation, WhereClip onWhere)
            where TJoin : Entity
        {
            return RightJoin(relation, null, onWhere);
        }

        /// <summary>
        /// 右连接查询
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="relation"></param>
        /// <param name="aliasName"></param>
        /// <param name="onWhere"></param>
        /// <returns></returns>
        public FromSection<T> RightJoin<TJoin>(TableRelation<TJoin> relation, string aliasName, WhereClip onWhere)
            where TJoin : Entity
        {
            return Join<TJoin>(relation, aliasName, onWhere, JoinType.RightJoin);
        }

        #endregion

        #region 私有方法

        private FromSection<T> Join<TJoin>(TableRelation<TJoin> relation, string aliasName, WhereClip onWhere, JoinType joinType)
            where TJoin : Entity
        {
            //将TableRelation的对象添加到当前节
            this.entities.AddRange(relation.GetFromSection().TableEntities);

            TJoin entity = CoreHelper.CreateInstance<TJoin>();
            var table = entity.GetTable().As(aliasName);

            if ((IField)query.PagingField == null)
            {
                //标识列和主键优先,包含ID的列被抛弃
                query.SetPagingField(entity.PagingField);
            }

            string tableName = entity.GetTable().Name;
            if (aliasName != null) tableName = aliasName;

            //处理tableRelation关系
            string joinString = "(" + relation.GetFromSection().Query.QueryString + ") " + tableName;
            this.query.Parameters = relation.GetFromSection().Query.Parameters;

            string strJoin = string.Empty;
            if (onWhere != null)
            {
                strJoin = " ON " + onWhere.ToString();
            }

            //获取关联方式
            string join = GetJoinEnumString(joinType);

            if (this.relation != null)
            {
                this.tableName = " __[[ " + this.tableName;
                this.relation += " ]]__ ";
            }
            this.relation += join + joinString + strJoin;

            return this;
        }

        private FromSection<T> Join<TJoin>(Table table, string aliasName, WhereClip onWhere, JoinType joinType)
            where TJoin : Entity
        {
            TJoin entity = CoreHelper.CreateInstance<TJoin>();
            table = table ?? entity.GetTable();
            table.As(aliasName);

            //创建一个TableEntity
            var tableEntity = new TableEntity
            {
                Table = table,
                Entity = entity
            };

            this.entities.Add(tableEntity);

            if ((IField)query.PagingField == null)
            {
                //标识列和主键优先,包含ID的列被抛弃
                query.SetPagingField(entity.PagingField);
            }

            string strJoin = string.Empty;
            if (onWhere != null)
            {
                strJoin = " ON " + onWhere.ToString();
            }

            //获取关联方式
            string join = GetJoinEnumString(joinType);

            if (this.relation != null)
            {
                this.tableName = " __[[ " + this.tableName;
                this.relation += " ]]__ ";
            }
            this.relation += join + table.FullName + strJoin;

            return this;
        }

        #endregion

        #endregion

        private string GetJoinEnumString(JoinType joinType)
        {
            switch (joinType)
            {
                case JoinType.LeftJoin:
                    return " LEFT OUTER JOIN ";
                case JoinType.RightJoin:
                    return " RIGHT OUTER JOIN ";
                case JoinType.InnerJoin:
                    return " INNER JOIN ";
                default:
                    return " INNER JOIN ";
            }
        }

        #region 公有方法

        internal Field GetPagingField()
        {
            foreach (TableEntity entity in this.entities)
            {
                var field = entity.Entity.PagingField;
                if ((IField)field != null)
                {
                    return field.At(entity.Table);
                }
            }

            return null;
        }

        internal Field[] GetSelectFields()
        {
            var dictfields = new Dictionary<string, Field>();
            foreach (TableEntity entity in this.entities)
            {
                Table table = entity.Table;
                Field[] fields = entity.Entity.GetFields();
                if (fields == null || fields.Length == 0)
                {
                    throw new DataException("没有任何被选中的字段列表！");
                }
                else
                {
                    foreach (Field field in fields)
                    {
                        //去除重复的字段
                        if (!dictfields.ContainsKey(field.OriginalName))
                        {
                            dictfields[field.OriginalName] = field.At(table);
                        }
                    }
                }
            }

            //返回选中的字段
            return dictfields.Select(p => p.Value).ToArray();
        }

        #endregion

        #region 内部变量

        private string tableName;
        internal string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                tableName = value;
            }
        }

        private string relation;
        internal string Relation
        {
            get
            {
                return relation;
            }
            set
            {
                relation = value;
            }
        }

        #endregion
    }
}
