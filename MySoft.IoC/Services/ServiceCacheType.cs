using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum ServiceCacheType
    {
        /// <summary>
        /// 不进行缓存
        /// </summary>
        None,
        /// <summary>
        /// 内存方式
        /// </summary>
        Memory,
        /// <summary>
        /// 文件方式
        /// </summary>
        File
    }
}
