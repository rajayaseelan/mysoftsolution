using System;
using MySoft.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存对象
    /// </summary>
    [Serializable]
    internal class CacheItem : CacheObject<byte[]>
    {
        /// <summary>
        /// 记录数
        /// </summary>
        public int Count { get; set; }
    }
}
