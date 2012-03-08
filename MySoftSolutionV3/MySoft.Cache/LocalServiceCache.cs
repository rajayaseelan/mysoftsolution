using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Cache
{
    /// <summary>
    /// 本地缓存
    /// </summary>
    public class LocalServiceCache : ServiceCacheBase
    {
        /// <summary>
        /// 实例化LocalServiceCache
        /// </summary>
        public LocalServiceCache()
            : base(CacheFactory.CreateCache("ServiceCache", CacheType.Local))
        { }
    }
}
