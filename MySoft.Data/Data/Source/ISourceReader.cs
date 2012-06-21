using System;
using System.Data;

namespace MySoft.Data
{
    /// <summary>
    /// 读数据接口
    /// </summary>
    interface ISourceReader : IListConvert<IRowReader>, IRowReader, IDataSource<IDataReader>, IDisposable
    {
        #region 常用方法

        /// <summary>
        /// 获取字段数
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// 关闭DbReader
        /// </summary>
        void Close();

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
        bool Read();

        /// <summary>
        /// 转换为SourceTable
        /// </summary>
        /// <returns></returns>
        SourceTable ToTable();

        /// <summary>
        /// 返回下一结果集
        /// </summary>
        /// <returns></returns>
        bool NextResult();

        #endregion
    }
}
