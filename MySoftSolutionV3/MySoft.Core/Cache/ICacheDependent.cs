using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Cache
{
    /// <summary>
    /// 缓存依赖
    /// </summary>
    public interface ICacheDependent
    {
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="cacheType"></param>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="cacheTime"></param>
        void AddCache(Type cacheType, string cacheKey, object cacheValue, double cacheTime);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheType"></param>
        /// <param name="cacheKey"></param>
        void RemoveCache(Type cacheType, string cacheKey);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="cacheType"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        object GetCache(Type cacheType, string cacheKey);

        #region 处理一组缓存

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheType"></param>
        void RemoveCache(Type cacheType);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        IList<object> GetCache(Type cacheType);

        #endregion
    }
}
