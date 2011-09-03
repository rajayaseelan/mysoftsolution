using System;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.Cache
{
    /// <summary>
    /// 默认缓存依赖
    /// </summary>
    public class DefaultCacheDependent : ICacheDependent
    {
        private ICacheStrategy strategy;
        /// <summary>
        /// 实例化默认缓存依赖
        /// </summary>
        /// <param name="strategy"></param>
        public DefaultCacheDependent(ICacheStrategy strategy)
        {
            this.strategy = strategy;
        }

        /// <summary>
        /// 实例化默认缓存依赖
        /// </summary>
        /// <param name="type"></param>
        public DefaultCacheDependent(CacheType type)
        {
            this.strategy = CacheFactory.CreateCache("DefaultCacheDependent", type);
        }

        #region ICacheDependent 成员

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="cacheTime"></param>
        public virtual void AddCache(Type serviceType, string cacheKey, object cacheValue, double cacheTime)
        {
            lock (strategy)
            {
                //组合CacheKey
                cacheKey = string.Format("{0}_{1}", serviceType.FullName, cacheKey);

                if (cacheTime > 0)
                {
                    strategy.AddObject(cacheKey, cacheValue, TimeSpan.FromSeconds(cacheTime));
                }
            }
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="cacheKey"></param>
        public virtual void RemoveCache(Type serviceType, string cacheKey)
        {
            lock (strategy)
            {
                //组合CacheKey
                cacheKey = string.Format("{0}_{1}", serviceType.FullName, cacheKey);

                strategy.RemoveObject(cacheKey);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual object GetCache(Type serviceType, string cacheKey)
        {
            lock (strategy)
            {
                //组合CacheKey
                cacheKey = string.Format("{0}_{1}", serviceType.FullName, cacheKey);

                return strategy.GetObject(cacheKey);
            }
        }

        #endregion

        #region 处理一组缓存

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="serviceType"></param>
        public virtual void RemoveCache(Type serviceType)
        {
            lock (strategy)
            {
                strategy.RemoveMatchObjects(serviceType.FullName);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public virtual IList<object> GetCache(Type serviceType)
        {
            lock (strategy)
            {
                return strategy.GetMatchObjects(serviceType.FullName).Values.ToList();
            }
        }

        #endregion
    }
}
