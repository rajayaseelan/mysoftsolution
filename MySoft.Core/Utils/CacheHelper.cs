using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Threading;
using MySoft.Logger;

namespace MySoft
{
    /// <summary>
    /// 缓存管理类
    /// </summary>
    public class CacheHelper
    {
        /// <summary>
        /// DayFactor
        /// </summary>
        public static readonly int DayFactor = 17280;
        /// <summary>
        /// HourFactor
        /// </summary>
        public static readonly int HourFactor = 720;
        /// <summary>
        /// MinuteFactor
        /// </summary>
        public static readonly int MinuteFactor = 12;
        /// <summary>
        /// SecondFactor
        /// </summary>
        public static readonly double SecondFactor = 0.2;

        private static readonly System.Web.Caching.Cache _cache;

        private static int Factor = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheFactor"></param>
        public static void ReSetFactor(int cacheFactor)
        {
            Factor = cacheFactor;
        }

        /// <summary>
        /// 确保当前HttpContext只有一个Cache实例
        /// </summary>
        static CacheHelper()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                _cache = context.Cache;
            }
            else
            {
                _cache = HttpRuntime.Cache;
            }
        }

        /// <summary>
        /// 清空Cache
        /// </summary>
        public static void Clear()
        {
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();

            while (CacheEnum.MoveNext())
            {
                _cache.Remove(CacheEnum.Key.ToString());
            }
        }

        /// <summary>
        /// 根据正则表达式的模式移除Cache
        /// </summary>
        /// <param name="pattern">模式</param>
        public static void RemoveByPattern(string pattern)
        {
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            while (CacheEnum.MoveNext())
            {
                if (regex.IsMatch(CacheEnum.Key.ToString()))
                    _cache.Remove(CacheEnum.Key.ToString());
            }
        }

        /// <summary>
        /// 根据键值移除Cache
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <summary>
        /// 把对象加载到Cache
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="obj">对象</param>
        public static void Insert(string key, object obj)
        {
            Insert(key, obj, null, 1);
        }

        /// <summary>
        /// 把对象加载到Cache,附加缓存依赖信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="dep"></param>
        public static void Insert(string key, object obj, CacheDependency dep)
        {
            Insert(key, obj, dep, MinuteFactor * 3);
        }

        /// <summary>
        /// 把对象加载到Cache,附加过期时间信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="seconds"></param>
        public static void Insert(string key, object obj, int seconds)
        {
            Insert(key, obj, null, seconds);
        }

        /// <summary>
        /// 把对象加载到Cache,附加过期时间信息和优先级
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="seconds"></param>
        /// <param name="priority"></param>
        public static void Insert(string key, object obj, int seconds, CacheItemPriority priority)
        {
            Insert(key, obj, null, seconds, priority);
        }

        /// <summary>
        /// 把对象加载到Cache,附加缓存依赖和过期时间(多少秒后过期)
        /// (默认优先级为High)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="dep"></param>
        /// <param name="seconds"></param>
        public static void Insert(string key, object obj, CacheDependency dep, int seconds)
        {
            Insert(key, obj, dep, seconds, CacheItemPriority.Normal);
        }

        /// <summary>
        /// 把对象加载到Cache,附加缓存依赖和过期时间(多少秒后过期)及优先级
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="dep"></param>
        /// <param name="seconds"></param>
        /// <param name="priority"></param>
        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, DateTime.Now.AddSeconds(Factor * seconds), System.Web.Caching.Cache.NoSlidingExpiration, priority, null);
            }

        }

        /// <summary>
        /// 把对象加到缓存并忽略优先级
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="seconds"></param>
        public static void MicroInsert(string key, object obj, int seconds)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, null, DateTime.Now.AddSeconds(Factor * seconds), System.Web.Caching.Cache.NoSlidingExpiration);
            }
        }

        /// <summary>
        /// 把对象加到缓存,并把过期时间设为最大值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Max(string key, object obj)
        {
            Max(key, obj, null);
        }

        /// <summary>
        /// 把对象加到缓存,并把过期时间设为最大值,附加缓存依赖信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="dep"></param>
        public static void Max(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);
            }
        }

        /// <summary>
        /// 插入持久性缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void Permanent(string key, object obj)
        {
            Permanent(key, obj, null);
        }

        /// <summary>
        /// 插入持久性缓存,附加缓存依赖
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="dep"></param>
        public static void Permanent(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                _cache.Insert(key, obj, dep, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
            }
        }

        /// <summary>
        /// 根据键获取被缓存的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(string key)
        {
            return _cache[key];
        }

        /// <summary>
        /// 根据键获取被缓存的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return (T)Get(key);
        }

        /// <summary>
        /// Return int of seconds * SecondFactor
        /// </summary>
        public static int SecondFactorCalculate(int seconds)
        {
            // Insert method below takes integer seconds, so we have to round any fractional values
            return Convert.ToInt32(Math.Round((double)seconds * SecondFactor));
        }
    }

    /// <summary>
    /// 缓存扩展类
    /// </summary>
    public static class CacheHelper<T>
    {
        //缓存倍数
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
            return Get(key, timeout, func, null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static T Get(string key, TimeSpan timeout, Func<T> func, Predicate<T> pred)
        {
            return Get(key, timeout, state => func(), null, pred);
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
            return Get(key, timeout, func, state, null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static T Get(string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
            var cacheObj = CacheHelper.Get(key);

            if (cacheObj == null)
            {
                var spareKey = string.Format("SpareCache_{0}", key);
                cacheObj = CacheHelper.Get(spareKey);

                if (cacheObj == null)
                {
                    try
                    {
                        cacheObj = func(state);
                    }
                    catch (ThreadInterruptedException ex) { }
                    catch (ThreadAbortException ex)
                    {
                        Thread.ResetAbort();
                    }

                    if (cacheObj != null)
                    {
                        var success = true;
                        if (pred != null)
                        {
                            try
                            {
                                success = pred((T)cacheObj);
                            }
                            catch
                            {
                            }
                        }

                        if (success)
                        {
                            CacheHelper.Insert(key, cacheObj, (int)timeout.TotalSeconds);
                            CacheHelper.Insert(spareKey, cacheObj, (int)TimeSpan.FromDays(1).TotalSeconds);
                        }
                    }
                }
                else
                {
                    lock (hashtable)
                    {
                        if (!hashtable.Contains(key))
                        {
                            hashtable.Add(key);
                            func.BeginInvoke(state, AsyncCallback, new ArrayList { key, timeout, func, pred });
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
                    var pred = arr[3] as Predicate<T>;

                    var cacheObj = func.EndInvoke(ar);

                    if (cacheObj != null)
                    {
                        var success = true;
                        if (pred != null)
                        {
                            try
                            {
                                success = pred(cacheObj);
                            }
                            catch
                            {
                                success = false;
                            }
                        }

                        if (success)
                        {
                            var spareKey = string.Format("SpareCache_{0}", key);
                            CacheHelper.Insert(key, cacheObj, (int)timeout.TotalSeconds);
                            CacheHelper.Insert(spareKey, cacheObj, (int)TimeSpan.FromDays(1).TotalSeconds);
                        }
                    }
                }
                catch (ThreadInterruptedException ex) { }
                catch (ThreadAbortException ex)
                {
                    Thread.ResetAbort();
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
                SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
            }
            finally
            {
                ar.AsyncWaitHandle.Close();
            }
        }
    }
}
