using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Cache
{
    /// <summary>
    /// 服务缓存基类
    /// </summary>
    public abstract class ServiceCacheBase : IServiceCache
    {
        private ICacheStrategy cache;

        /// <summary>
        /// 实例化ServiceCacheBase
        /// </summary>
        /// <param name="cache"></param>
        public ServiceCacheBase(ICacheStrategy cache)
        {
            this.cache = cache;
        }

        #region IServiceCache 成员

        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        public void Insert(string key, object value, int seconds)
        {
            cache.AddObject(key, value, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return cache.GetObject<T>(key);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            cache.RemoveObject(key);
        }

        #endregion
    }
}
