using System.Data;

namespace MySoft.Data
{
    /// <summary>
    /// Top对应的Query查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TopSection<T> : QuerySection<T>
        where T : Entity
    {
        private QuerySection<T> query;
        private int topSize;

        internal TopSection(QuerySection<T> query, DbProvider dbProvider, DbTrans dbTran, int topSize)
            : base(query.FromSection, dbProvider, dbTran)
        {
            this.query = query;
            this.topSize = topSize;
        }

        /// <summary>
        /// QueryString
        /// </summary>
        internal override string QueryString
        {
            get
            {
                return query.QueryString;
            }
            set
            {
                query.QueryString = value;
            }
        }

        /// <summary>
        /// CreateQuery
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        internal override QuerySection<TResult> CreateQuery<TResult>()
        {
            return query.SubQuery("SUB_QUERY_TABLE").CreateQuery<TResult>();
        }

        #region 方法重载

        /// <summary>
        /// 获取分页
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public override PageSection<T> GetPage(int pageSize)
        {
            return query.SubQuery("SUB_TOP_PAGE_TABLE").GetPage(pageSize);
        }

        /// <summary>
        /// 返回结果列表
        /// </summary>
        /// <returns></returns>
        public override ArrayList<object> ToListResult()
        {
            return query.ToListResult(0, topSize);
        }

        /// <summary>
        /// 返回结果列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public override ArrayList<TResult> ToListResult<TResult>()
        {
            return query.ToListResult<TResult>(0, topSize);
        }

        /// <summary>
        /// 返回实体列表
        /// </summary>
        /// <returns></returns>
        public override SourceList<T> ToList()
        {
            return query.ToList(0, topSize);
        }

        /// <summary>
        /// 返回实体列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public override SourceList<TResult> ToList<TResult>()
        {
            return query.ToList<TResult>(0, topSize);
        }

        /// <summary>
        /// 返回阅读器
        /// </summary>
        /// <returns></returns>
        public override SourceReader ToReader()
        {
            return query.ToReader(0, topSize);
        }

        /// <summary>
        /// 返回表数据
        /// </summary>
        /// <returns></returns>
        public override SourceTable ToTable()
        {
            return query.ToTable(0, topSize);
        }

        /// <summary>
        /// 返回数据集
        /// </summary>
        /// <returns></returns>
        public override DataSet ToDataSet()
        {
            return query.ToDataSet(0, topSize);
        }

        /// <summary>
        /// 创建子查询
        /// </summary>
        /// <returns></returns>
        public override QuerySection<T> SubQuery()
        {
            return query.SubQuery();
        }

        /// <summary>
        /// 创建子查询
        /// </summary>
        /// <param name="aliasName"></param>
        /// <returns></returns>
        public override QuerySection<T> SubQuery(string aliasName)
        {
            return query.SubQuery(aliasName);
        }

        /// <summary>
        /// 创建子查询
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <returns></returns>
        public override QuerySection<TSub> SubQuery<TSub>()
        {
            return query.SubQuery<TSub>();
        }

        /// <summary>
        /// 创建子查询
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <param name="aliasName"></param>
        /// <returns></returns>
        public override QuerySection<TSub> SubQuery<TSub>(string aliasName)
        {
            return query.SubQuery<TSub>(aliasName);
        }

        #endregion
    }
}
