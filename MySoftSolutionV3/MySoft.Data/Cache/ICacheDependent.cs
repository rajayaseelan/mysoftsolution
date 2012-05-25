using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data.Cache
{
    /// <summary>
    /// 缓存依赖
    /// </summary>
    public interface ICacheDependent
    {
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="cacheTime"></param>
        void AddCache<T>(string cacheKey, T cacheValue, int cacheTime);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        void RemoveCache<T>(string cacheKey);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T GetCache<T>(string cacheKey);
    }
}
