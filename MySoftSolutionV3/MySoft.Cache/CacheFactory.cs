using System.Configuration;

namespace MySoft.Cache
{
    /// <summary>
    /// 缓存工厂类
    /// </summary>
    public static class CacheFactory
    {
        /// <summary>
        /// 创建一个缓存块
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static ICacheStrategy CreateCache(string configName)
        {
            return CreateCache(null, configName);
        }

        /// <summary>
        /// 创建一个缓存块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ICacheStrategy CreateCache(CacheType type)
        {
            return CreateCache(null, type);
        }

        /// <summary>
        /// 创建一个缓存块(用前缀区分)
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static ICacheStrategy CreateCache(string regionName, string configName)
        {
            var name = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrEmpty(name))
            {
                name = "Local"; //默认为本地
            }

            if (name.ToUpper() == "LOCAL")
                return CreateCache(regionName, CacheType.Local);
            else
                return CreateCache(regionName, CacheType.Distributed);
        }

        /// <summary>
        /// 创建一个缓存块(用前缀区分)
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ICacheStrategy CreateCache(string regionName, CacheType type)
        {
            //创建缓存块
            switch (type)
            {
                case CacheType.Local:
                    return new LocalCacheStrategy(regionName);
                case CacheType.Distributed:
                    return new SharedCacheStrategy(regionName);
                default:
                    return new LocalCacheStrategy(regionName);
            }
        }
    }
}
