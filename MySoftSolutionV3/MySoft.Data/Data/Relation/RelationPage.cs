using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Data.Design;

namespace MySoft.Data
{
    /// <summary>
    /// 关系分页
    /// </summary>
    public class RelationPage<T>
        where T : class
    {
        private QuerySection<ViewEntity> query;
        private int? rowCount;
        private int pageSize;
        internal RelationPage(QuerySection<ViewEntity> query, int pageSize)
        {
            this.query = query;
            this.pageSize = pageSize;
        }

        /// <summary>
        /// 返回页数
        /// </summary>
        public int PageCount
        {
            get
            {
                if (rowCount == null)
                {
                    rowCount = query.Count();
                }
                return Convert.ToInt32(Math.Ceiling(1.0 * rowCount.Value / pageSize));
            }
        }

        /// <summary>
        /// 返回记录数
        /// </summary>
        public int RowCount
        {
            get
            {
                if (rowCount == null)
                {
                    rowCount = query.Count();
                }
                return rowCount.Value;
            }
        }

        /// <summary>
        /// 返回ISourceReader
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public SourceReader ToReader(int pageIndex)
        {
            return query.GetPage(pageSize).ToReader(pageIndex);
        }

        /// <summary>
        /// 返回IArrayList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public SourceList<T> ToList(int pageIndex)
        {
            return ToTable(pageIndex).ConvertTo<T>();
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public SourceTable ToTable(int pageIndex)
        {
            return query.GetPage(pageSize).ToTable(pageIndex);
        }
    }
}
