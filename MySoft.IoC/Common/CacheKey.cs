using System;

namespace MySoft.IoC
{
    internal class CacheKey
    {
        /// <summary>
        /// 缓存唯一Id
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }
    }
}
