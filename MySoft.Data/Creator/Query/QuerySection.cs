using System.Collections.Generic;
using System.Data;
using MySoft.Data.Design;

namespace MySoft.Data
{
    /// <summary>
    /// 查询器
    /// </summary>
    public class QuerySection : IUserQuery
    {
        private QuerySection<ViewEntity> query;
        internal QuerySection(QuerySection<ViewEntity> query)
        {
            this.query = query;
        }

        /// <summary>
        /// 设置分页字段
        /// </summary>
        /// <param name="pagingFieldName"></param>
        /// <returns></returns>
        public QuerySection SetPagingField(string pagingFieldName)
        {
            return SetPagingField(new Field(pagingFieldName));
        }

        /// <summary>
        /// 设置分页字段
        /// </summary>
        /// <param name="pagingField"></param>
        /// <returns></returns>
        public QuerySection SetPagingField(Field pagingField)
        {
            query.SetPagingField(pagingField);
            return this;
        }

        /// <summary>
        /// 获取分页
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PageSection GetPage(int pageSize)
        {
            return new PageSection(query, pageSize);
        }

        /// <summary>
        /// 返回首行首列值
        /// </summary>
        /// <returns></returns>
        public object ToScalar()
        {
            return query.ToScalar();
        }

        /// <summary>
        /// 返回首行首列值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToScalar<TResult>()
        {
            return query.ToScalar<TResult>();
        }

        /// <summary>
        /// 返回记录数
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

        /// <summary>
        /// 记录是否存在
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            return query.Count() > 0;
        }

        /// <summary>
        /// 返回T
        /// </summary>
        /// <returns></returns>
        public T ToSingle<T>()
            where T : class
        {
            return query.GetTop(1).ToReader().ConvertTo<T>()[0];
        }

        /// <summary>
        /// 返回IList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SourceList<T> ToList<T>()
            where T : class
        {
            return query.ToReader().ConvertTo<T>();
        }

        /// <summary>
        /// 返回ISourceReader
        /// </summary>
        /// <returns></returns>
        public SourceReader ToReader()
        {
            return query.ToReader();
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <returns></returns>
        public SourceTable ToTable()
        {
            return query.ToTable();
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <returns></returns>
        public DataSet ToDataSet()
        {
            return query.ToDataSet();
        }

        /// <summary>
        /// 返回ISourceReader
        /// </summary>
        /// <returns></returns>
        public SourceReader ToReader(int topSize)
        {
            return query.GetTop(topSize).ToReader();
        }

        /// <summary>
        /// 返回IList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SourceList<T> ToList<T>(int topSize)
            where T : class
        {
            return ToReader(topSize).ConvertTo<T>();
        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <returns></returns>
        public SourceTable ToTable(int topSize)
        {
            return query.GetTop(topSize).ToTable();
        }

        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <returns></returns>
        public DataSet ToDataSet(int topSize)
        {
            return query.GetTop(topSize).ToDataSet();
        }

        /// <summary>
        /// 返回DataPage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataPage<IList<T>> ToListPage<T>(int pageSize, int pageIndex)
            where T : class
        {
            DataPage<IList<T>> view = new DataPage<IList<T>>(pageSize);
            PageSection page = GetPage(pageSize);
            view.CurrentPageIndex = pageIndex;
            view.RowCount = page.RowCount;
            view.DataSource = page.ToList<T>(pageIndex);
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
            PageSection page = GetPage(pageSize);
            view.CurrentPageIndex = pageIndex;
            view.RowCount = page.RowCount;
            view.DataSource = page.ToTable(pageIndex);
            return view;
        }


        /// <summary>
        /// 返回DataPage
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataPage<DataSet> ToDataSetPage(int pageSize, int pageIndex)
        {
            DataPage<DataSet> view = new DataPage<DataSet>(pageSize);
            PageSection page = GetPage(pageSize);
            view.CurrentPageIndex = pageIndex;
            view.RowCount = page.RowCount;
            view.DataSource = page.ToDataSet(pageIndex);
            return view;
        }
    }
}
