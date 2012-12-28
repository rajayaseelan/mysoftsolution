using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Cache;

namespace MySoft.PlatformService.WinForm
{
    internal class MonitorCache : IDataCache
    {
        #region IDataCache 成员

        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        public void Insert(string key, object value, TimeSpan timeout)
        {
            CacheHelper.Insert(key, value, (int)timeout.TotalSeconds);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return CacheHelper.Get<T>(key);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            CacheHelper.Remove(key);
        }

        #endregion
    }
}
