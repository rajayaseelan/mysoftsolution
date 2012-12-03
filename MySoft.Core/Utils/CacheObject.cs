using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft
{
    /// <summary>
    /// 缓存对象
    /// </summary>
    [Serializable]
    internal class CacheObject<T>
    {
        /// <summary>
        /// 缓存对象
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Expired { get; set; }
    }
}
