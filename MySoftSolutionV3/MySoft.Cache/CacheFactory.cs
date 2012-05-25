using System.Configuration;
using System;

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
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static ICacheStrategy Create(string typeName)
        {
            return Create(null, typeName);
        }

        /// <summary>
        /// 创建一个缓存块
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ICacheStrategy Create(CacheType type)
        {
            return Create(null, type);
        }

        /// <summary>
        /// 创建一个缓存块(用前缀区分)
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static ICacheStrategy Create(string bucketName, string typeName)
        {
            var cacheType = Type.GetType(typeName);

            //获取类型及创建缓存服务
            return (ICacheStrategy)Activator.CreateInstance(cacheType, new object[] { bucketName });
        }

        /// <summary>
        /// 创建一个缓存块(用前缀区分)
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ICacheStrategy Create(string bucketName, CacheType type)
        {
            //创建缓存块
            switch (type)
            {
                case CacheType.Local:
                    return new LocalCacheStrategy(bucketName);
                case CacheType.Distributed:
                    {
                        var distributedType = ConfigurationManager.AppSettings["DistributedType"];
                        return Create(bucketName, GetTypeName(distributedType));
                    }
                default:
                    return new LocalCacheStrategy(bucketName);
            }
        }

        /// <summary>
        /// 获取类型名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTypeName(string type)
        {
            if (string.IsNullOrEmpty(type)) type = "shared";

            switch (type.ToLower())
            {
                case "shared":
                    return "MySoft.Cache.SharedCacheStrategy, MySoft.Cache.Shared";
                case "couch":
                    return "MySoft.Cache.CouchCacheStrategy, MySoft.Cache.Couch";
                default:
                    return "MySoft.Cache.SharedCacheStrategy, MySoft.Cache.Shared";
            }
        }
    }
}
