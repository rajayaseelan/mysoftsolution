using MySoft.Data.Design;
using MySoft.Data.SqlServer;

namespace MySoft.Data.SqlServer9
{
    /// <summary>
    /// SQL Server 2005 驱动
    /// </summary>
    public class SqlServer9Provider : SqlServerProvider
    {
        public SqlServer9Provider(string connectionString)
            : base(connectionString)
        { }

        /// <summary>
        /// 创建分页查询
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        protected internal override QuerySection<T> CreatePageQuery<T>(QuerySection<T> query, int itemCount, int skipCount)
        {
            if (skipCount == 0)
            {
                ((IPaging)query).Prefix("TOP " + itemCount);
                return query;
            }
            else
            {
                //如果没有指定Order 则由指定的key来排序
                if (query.OrderString == null)
                {
                    Field pagingField = query.PagingField;

                    if ((IField)pagingField == null)
                    {
                        throw new DataException("请指定分页主键或设置排序！");
                    }

                    query.OrderBy(pagingField.Asc);
                }

                ((IPaging)query).Suffix(",ROW_NUMBER() OVER(" + query.OrderString + ") AS TMP__ROWID");
                query.OrderBy(OrderByClip.None);

                QuerySection<T> jquery = query.SubQuery("TMP_TABLE");
                jquery.Where(new WhereClip("TMP__ROWID BETWEEN " + (skipCount + 1) + " AND " + (itemCount + skipCount)));
                jquery.Select(Field.All.At("TMP_TABLE"));

                return jquery;
            }
        }
    }
}
