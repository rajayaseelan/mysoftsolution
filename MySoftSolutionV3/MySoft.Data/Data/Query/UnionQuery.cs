using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Data.Design;

namespace MySoft.Data
{
    /// <summary>
    /// 联合查询
    /// </summary>
    internal class UnionQuery<T> : QuerySection<T>
        where T : Entity
    {
        private Table unionTable = new Table("SUB_UNION_QUERY");
        private IList<UnionItem<T>> queries = new List<UnionItem<T>>();

        /// <summary>
        /// UnionQuery构造函数
        /// </summary>
        /// <param name="query1"></param>
        /// <param name="query2"></param>
        /// <param name="dbProvider"></param>
        /// <param name="dbTran"></param>
        /// <param name="isUnionAll"></param>
        public UnionQuery(QuerySection<T> query1, QuerySection<T> query2, DbProvider dbProvider, DbTrans dbTran, bool isUnionAll)
            : base(query1.FromSection, dbProvider, dbTran)
        {
            this.queries.Add(new UnionItem<T> { Query = query1, IsUnionAll = isUnionAll });
            this.queries.Add(new UnionItem<T> { Query = query2, IsUnionAll = isUnionAll });
        }

        #region 方法重载

        internal override string CountString
        {
            get
            {
                var sb = new StringBuilder();
                int index = 0;
                foreach (var q in queries)
                {
                    sb.Append(q.Query.CountString);
                    index++;

                    if (index < queries.Count)
                    {
                        sb.Append(q.IsUnionAll ? " UNION ALL " : " UNION ");
                    }
                }

                return string.Format("SELECT SUM({0}.ROW_COUNT) AS ROW_COUNT FROM ({1}) {0}", "SUB_UNION_QUERY", sb.ToString());
            }
        }

        /// <summary>
        /// QueryString
        /// </summary>
        internal override string QueryString
        {
            get
            {
                var sb = new StringBuilder();
                int index = 0;
                foreach (var q in queries)
                {
                    sb.Append(q.Query.QueryString);
                    index++;

                    if (index < queries.Count)
                    {
                        sb.Append(q.IsUnionAll ? " UNION ALL " : " UNION ");
                    }
                }


                var sqlString = string.Format("({0}) {1}", sb.ToString(), unionTable.Name);

                //处理联合查询的SQL
                return string.Format(formatString, distinctString, prefixString, Field.All.At(unionTable).Name,
                    suffixString, sqlString, null, null, null, OrderString, endString);
            }
        }

        /// <summary>
        /// 排序信息
        /// </summary>
        internal override string OrderString
        {
            get
            {
                if ((IField)PagingField != null)
                    return " ORDER BY " + PagingField.At(unionTable).Asc.ToString();
                else
                    return null;
            }
        }

        internal override SQLParameter[] Parameters
        {
            get
            {
                var list = new List<SQLParameter>();
                foreach (var q in queries)
                {
                    list.AddRange(q.Query.Parameters);
                }

                return list.ToArray();
            }
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override QuerySection<T> Union(QuerySection<T> query)
        {
            this.queries.Add(new UnionItem<T> { Query = query, IsUnionAll = false });
            return this;
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override QuerySection<T> UnionAll(QuerySection<T> query)
        {
            this.queries.Add(new UnionItem<T> { Query = query, IsUnionAll = true });
            return this;
        }

        #endregion
    }

    /// <summary>
    /// 联合查询Item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class UnionItem<T> where T : Entity
    {
        /// <summary>
        /// 查询节
        /// </summary>
        public QuerySection<T> Query { get; set; }

        /// <summary>
        /// 是否全部Union
        /// </summary>
        public bool IsUnionAll { get; set; }
    }
}
