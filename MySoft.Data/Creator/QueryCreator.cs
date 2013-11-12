using System;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// TableJoin
    /// </summary>
    [Serializable]
    internal class TableJoin
    {
        public Table Table { get; set; }
        public JoinType Type { get; set; }
        public WhereClip Where { get; set; }
    }

    /// <summary>
    /// 查询创建器
    /// </summary>
    [Serializable]
    public class QueryCreator : WhereCreator<QueryCreator>, IQueryCreator
    {
        /// <summary>
        /// 创建一个新的查询器（条件为全部，排序为默认)
        /// </summary>
        public static QueryCreator NewCreator(string tableName)
        {
            return new QueryCreator(tableName, null);
        }

        /// <summary>
        /// 创建一个新的查询器（条件为全部，排序为默认)
        /// </summary>
        public static QueryCreator NewCreator(string tableName, string aliasName)
        {
            return new QueryCreator(tableName, aliasName);
        }

        /// <summary>
        /// 创建一个新的查询器（条件为全部，排序为默认)
        /// </summary>
        public static QueryCreator NewCreator(Table table)
        {
            return new QueryCreator(table);
        }

        private IDictionary<string, TableJoin> joinTables;
        private List<OrderByClip> orderList;
        private List<Field> fieldList;

        /// <summary>
        /// 实例化QueryCreator
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="aliasName"></param>
        private QueryCreator(string tableName, string aliasName)
            : base(tableName, aliasName)
        {
            this.orderList = new List<OrderByClip>();
            this.fieldList = new List<Field>();
            this.joinTables = new Dictionary<string, TableJoin>();
        }

        /// <summary>
        /// 实例化QueryCreator
        /// </summary>
        /// <param name="table"></param>
        private QueryCreator(Table table)
            : base(table)
        {
            this.orderList = new List<OrderByClip>();
            this.fieldList = new List<Field>();
            this.joinTables = new Dictionary<string, TableJoin>();
        }

        #region 内部属性

        /// <summary>
        /// 返回排序
        /// </summary>
        internal OrderByClip OrderBy
        {
            get
            {
                OrderByClip newOrder = OrderByClip.None;
                foreach (OrderByClip order in orderList)
                {
                    newOrder &= order;
                }
                return newOrder;
            }
        }

        /// <summary>
        /// 返回字段列表
        /// </summary>
        internal Field[] Fields
        {
            get
            {
                if (fieldList.Count == 0)
                {
                    return new Field[] { Field.All.At(base.Table) };
                }
                return fieldList.ToArray();
            }
        }

        /// <summary>
        /// 是否关联查询
        /// </summary>
        internal bool IsRelation
        {
            get
            {
                return joinTables.Count > 0;
            }
        }

        /// <summary>
        /// 获取关联信息
        /// </summary>
        internal IDictionary<string, TableJoin> Relations
        {
            get
            {
                return joinTables;
            }
        }

        #endregion

        #region 表关联处理

        /// <summary>
        /// 关联表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public QueryCreator Join(string tableName, string where, params SQLParameter[] parameters)
        {
            return Join(JoinType.LeftJoin, tableName, null, where, parameters);
        }

        /// <summary>
        /// 关联表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="aliasName"></param>
        /// <param name="where"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public QueryCreator Join(string tableName, string aliasName, string where, params SQLParameter[] parameters)
        {
            return Join(JoinType.LeftJoin, tableName, aliasName, where, parameters);
        }

        /// <summary>
        /// 关联表信息
        /// </summary>
        /// <param name="table"></param>
        /// <param name="where"></param>
        public QueryCreator Join(Table table, WhereClip where)
        {
            return Join(JoinType.LeftJoin, table, where);
        }

        /// <summary>
        /// 关联表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public QueryCreator Join(JoinType joinType, string tableName, string where, params SQLParameter[] parameters)
        {
            return Join(joinType, tableName, null, where, parameters);
        }

        /// <summary>
        /// 关联表信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="aliasName"></param>
        /// <param name="where"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public QueryCreator Join(JoinType joinType, string tableName, string aliasName, string where, params SQLParameter[] parameters)
        {
            Table t = new Table(tableName).As(aliasName);

            if (!this.joinTables.ContainsKey(t.OriginalName))
            {
                TableJoin join = new TableJoin()
                {
                    Table = t,
                    Type = joinType,
                    Where = new WhereClip(where, parameters)
                };
                this.joinTables.Add(t.OriginalName, join);
            }
            return this;
        }

        /// <summary>
        /// 关联表信息
        /// </summary>
        /// <param name="table"></param>
        /// <param name="where"></param>
        public QueryCreator Join(JoinType joinType, Table table, WhereClip where)
        {
            if (!this.joinTables.ContainsKey(table.OriginalName))
            {
                TableJoin join = new TableJoin()
                {
                    Table = table,
                    Type = JoinType.LeftJoin,
                    Where = where
                };
                this.joinTables.Add(table.OriginalName, join);
            }
            return this;
        }

        #endregion

        #region 增加一个排序

        /// <summary>
        /// 添加一个排序
        /// </summary>
        /// <param name="order"></param>
        public QueryCreator AddOrder(OrderByClip order)
        {
            if (DataHelper.IsNullOrEmpty(order)) return this;

            if (orderList.Exists(o =>
            {
                return string.Compare(order.ToString(), o.ToString()) == 0;
            }))
            {
                return this;
            }

            //不存在条件，则加入
            orderList.Add(order);

            return this;
        }

        /// <summary>
        /// 添加一个排序
        /// </summary>
        /// <param name="orderby"></param>
        public QueryCreator AddOrder(string orderby)
        {
            if (string.IsNullOrEmpty(orderby)) return this;

            return AddOrder(new OrderByClip(orderby));
        }

        /// <summary>
        /// 增加排序规则
        /// </summary>
        /// <param name="field"></param>
        /// <param name="desc"></param>
        public QueryCreator AddOrder(Field field, bool desc)
        {
            if (desc)
                return AddOrder(field.Desc);
            else
                return AddOrder(field.Asc);
        }

        /// <summary>
        /// 增加排序规则
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="desc"></param>
        public QueryCreator AddOrder(string fieldName, bool desc)
        {
            return AddOrder(new Field(fieldName), desc);
        }

        #endregion

        #region 增加字段

        /// <summary>
        /// 添加一个字段
        /// </summary>
        /// <param name="field"></param>
        public QueryCreator AddField(Field field)
        {
            if (fieldList.Exists(f =>
            {
                return string.Compare(field.Name, f.Name) == 0;
            }))
            {
                return this;
            }

            fieldList.Add(field);

            return this;
        }

        /// <summary>
        /// 添加一个字段
        /// </summary>
        /// <param name="fieldName"></param>
        public QueryCreator AddField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return this;

            return AddField(new Field(fieldName));
        }

        /// <summary>
        /// 添加一个字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        public QueryCreator AddField(string tableName, string fieldName)
        {
            return AddField(new Field(fieldName).At(new Table(tableName)));
        }

        /// <summary>
        /// 移除指定的列
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public QueryCreator RemoveField(params Field[] fields)
        {
            if (fields == null) return this;

            foreach (Field field in fields)
            {
                int count = this.fieldList.RemoveAll(f =>
                {
                    return string.Compare(f.OriginalName, field.OriginalName, true) == 0;
                });

                if (count == 0)
                {
                    throw new DataException("指定的字段不存在于Query列表中！");
                }
            }

            return this;
        }

        /// <summary>
        /// 移除指定的列
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        public QueryCreator RemoveField(params string[] fieldNames)
        {
            if (fieldNames == null) return this;

            List<Field> fields = new List<Field>();
            foreach (string fieldName in fieldNames)
            {
                fields.Add(new Field(fieldName));
            }

            return RemoveField(fields.ToArray());
        }

        #endregion
    }
}
