using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Cache
{
    /// <summary>
    /// 分布式缓存
    /// </summary>
    public class SharedServiceCache : ServiceCacheBase
    {
        /// <summary>
        /// 实例化SharedServiceCache
        /// </summary>
        public SharedServiceCache()
            : base(CacheFactory.CreateCache("ServiceCache", CacheType.Distributed))
        { }
    }
}
