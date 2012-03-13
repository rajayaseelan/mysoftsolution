using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Cache;

namespace MySoft.IoC.Cache
{
    /// <summary>
    /// 服务缓存
    /// </summary>
    internal class CastleServiceCache : IServiceCache
    {
        private ICacheStrategy cache;

        /// <summary>
        /// 实例化ServiceCache
        /// </summary>
        /// <param name="cache"></param>
        public CastleServiceCache(ICacheStrategy cache)
        {
            this.cache = cache;
        }

        #region ICache 成员

        /// <summary>
        /// 插入缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        public void Insert(string key, object value, int seconds)
        {
            if (cache == null)
                CacheHelper.Insert(key, value, seconds);
            else
                cache.AddObject(key, value, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (cache == null)
                return CacheHelper.Get<T>(key);
            else
                return cache.GetObject<T>(key);
        }

        /// <summary>
        /// 移除缓存数据
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (cache == null)
                CacheHelper.Remove(key);
            else
                cache.RemoveObject(key);
        }

        #endregion
    }
}
