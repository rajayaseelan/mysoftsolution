using System;
using System.Collections.Generic;
using System.Linq;
using MySoft.Cache;

namespace MySoft.Data.Cache
{
    /// <summary>
    /// 默认缓存依赖
    /// </summary>
    internal class DataCacheDependent : ICacheDependent
    {
        private IDataCache strategy;
        private string connectName;

        /// <summary>
        /// 实例化默认缓存依赖
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="connectName"></param>
        public DataCacheDependent(IDataCache strategy, string connectName)
        {
            this.strategy = strategy;
            this.connectName = connectName;
        }

        #region ICacheDependent 成员

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="cacheTime"></param>
        public void AddCache<T>(string cacheKey, T cacheValue, int cacheTime)
        {
            if (cacheTime > 0)
            {
                //组合CacheKey
                cacheKey = string.Format("{0}${1}${2}", connectName, typeof(T).FullName, cacheKey);
                strategy.Insert(cacheKey, cacheValue, TimeSpan.FromSeconds(cacheTime));
            }
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        public void RemoveCache<T>(string cacheKey)
        {
            //组合CacheKey
            cacheKey = string.Format("{0}${1}${2}", connectName, typeof(T).FullName, cacheKey);
            strategy.Remove(cacheKey);
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="?"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public T GetCache<T>(string cacheKey)
        {
            //组合CacheKey
            cacheKey = string.Format("{0}${1}${2}", connectName, typeof(T).FullName, cacheKey);
            return strategy.Get<T>(cacheKey);
        }

        #endregion
    }
}
