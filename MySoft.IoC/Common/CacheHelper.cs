using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    internal static class CacheHelper<T>
    {
        //缓存倍数
        private const int CACHE_MULTIPLE = 10;
        private static readonly HashSet<string> hashtable = new HashSet<string>();

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T Get(string key, TimeSpan timeout, Func<T> func)
        {
            return Get(key, timeout, state => func(), null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static T Get(string key, TimeSpan timeout, Func<object, T> func, object state)
        {
            var cacheObj = CacheHelper.Get(key);

            if (cacheObj == null)
            {
                var spareKey = string.Format("SpareCache_{0}", key);
                cacheObj = CacheHelper.Get(spareKey);

                if (cacheObj == null)
                {
                    cacheObj = func(state);

                    if (cacheObj != null)
                    {
                        CacheHelper.Insert(key, cacheObj, (int)timeout.TotalSeconds);
                        CacheHelper.Insert(spareKey, cacheObj, (int)timeout.TotalSeconds * CACHE_MULTIPLE);
                    }
                }
                else
                {
                    lock (hashtable)
                    {
                        if (!hashtable.Contains(key))
                        {
                            hashtable.Add(key);
                            func.BeginInvoke(state, AsyncCallback, new ArrayList { key, timeout, func });
                        }
                    }
                }
            }

            if (cacheObj == null) return default(T);

            return (T)cacheObj;
        }

        /// <summary>
        /// 缓存回调
        /// </summary>
        /// <param name="ar"></param>
        private static void AsyncCallback(IAsyncResult ar)
        {
            var arr = ar.AsyncState as ArrayList;

            try
            {
                var key = Convert.ToString(arr[0]);

                try
                {
                    var timeout = (TimeSpan)arr[1];
                    var func = arr[2] as Func<object, T>;
                    var cacheObj = func.EndInvoke(ar);

                    if (cacheObj != null)
                    {
                        var spareKey = string.Format("SpareCache_{0}", key);
                        CacheHelper.Insert(key, cacheObj, (int)timeout.TotalSeconds);
                        CacheHelper.Insert(spareKey, cacheObj, (int)timeout.TotalSeconds * CACHE_MULTIPLE);
                    }
                }
                finally
                {
                    lock (hashtable)
                    {
                        hashtable.Remove(key);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ar.AsyncWaitHandle.Close();
            }
        }
    }
}
