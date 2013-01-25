using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using MySoft.Logger;
using MySoft.Security;
using MySoft.Threading;

namespace MySoft
{
    /// <summary>
    /// 缓存管理类
    /// </summary>
    public static class CacheHelper
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
        #region 数据缓存

        /// <summary>
        /// Hashtable.
        /// </summary>
        private static readonly Hashtable hashtable = new Hashtable();

        /// <summary>
        /// 从文件读取对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CacheObject<T> GetCache(string filePath)
        {
            var key = Path.GetFileNameWithoutExtension(filePath);

            //从文件读取对象
            return GetCache(LocalCacheType.File, filePath, key, TimeSpan.MaxValue);
        }

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
            return Get(LocalCacheType.Memory, key, timeout, func);
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
            return Get(LocalCacheType.Memory, key, timeout, func, pred);
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
            return Get(LocalCacheType.Memory, key, timeout, func, state);
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
            return Get(LocalCacheType.Memory, key, timeout, func, state, pred);
        }

        #endregion

        #region 设置缓存方式

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T Get(LocalCacheType type, string key, TimeSpan timeout, Func<T> func)
        {
            return Get(type, key, timeout, func, null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static T Get(LocalCacheType type, string key, TimeSpan timeout, Func<T> func, Predicate<T> pred)
        {
            return Get(type, key, timeout, state => func(), null, pred);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static T Get(LocalCacheType type, string key, TimeSpan timeout, Func<object, T> func, object state)
        {
            return Get(type, key, timeout, func, state, null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static T Get(LocalCacheType type, string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
            var cacheObj = GetCache(type, GetFilePath(key), key, timeout);

            if (cacheObj == null)
            {
                T internalObject = default(T);

                try
                {
                    internalObject = func(state);
                }
                catch (ThreadInterruptedException ex) { }
                catch (ThreadAbortException ex)
                {
                    Thread.ResetAbort();
                }

                if (internalObject != null)
                {
                    var success = true;
                    if (pred != null)
                    {
                        try
                        {
                            success = pred(internalObject);
                        }
                        catch
                        {
                            success = false;
                        }
                    }

                    if (success)
                    {
                        InsertCache(type, GetFilePath(key), key, internalObject, timeout);
                    }

                    return internalObject;
                }
            }
            else
            {
                //如果数据过期，则更新之
                if (cacheObj.ExpiredTime < DateTime.Now)
                {
                    lock (hashtable.SyncRoot)
                    {
                        if (!hashtable.ContainsKey(key))
                        {
                            hashtable[key] = new ArrayList { type, timeout, func, state, pred };

                            //异步更新
                            ManagedThreadPool.QueueUserWorkItem(WaitCallback, key);
                        }
                    }
                }
            }

            if (cacheObj == null) return default(T);

            return cacheObj.Value;
        }

        #endregion

        /// <summary>
        /// 缓存回调
        /// </summary>
        /// <param name="state"></param>
        private static void WaitCallback(object state)
        {
            var key = Convert.ToString(state);
            if (string.IsNullOrEmpty(key)) return;

            try
            {
                var arr = hashtable[key] as ArrayList;
                var _type = (LocalCacheType)arr[0];
                var _timeout = (TimeSpan)arr[1];
                var _func = arr[2] as Func<object, T>;
                var _state = arr[3];
                var _pred = arr[4] as Predicate<T>;

                var internalObject = _func(_state);

                if (internalObject != null)
                {
                    var success = true;
                    if (_pred != null)
                    {
                        try
                        {
                            success = _pred(internalObject);
                        }
                        catch
                        {
                            success = false;
                        }
                    }

                    if (success)
                    {
                        InsertCache(_type, GetFilePath(key), key, internalObject, _timeout);
                    }
                }
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
            }
            finally
            {
                lock (hashtable.SyncRoot)
                {
                    hashtable.Remove(key);
                }
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheObject<T> GetCache(LocalCacheType type, string path, string key, TimeSpan timeout)
        {
            var cacheObj = CacheHelper.Get<CacheObject<T>>(key);

            if (type == LocalCacheType.File && cacheObj == null)
            {
                try
                {
                    lock (hashtable.SyncRoot)
                    {
                        if (File.Exists(path))
                        {
                            var buffer = File.ReadAllBytes(path);
                            buffer = CompressionManager.DecompressGZip(buffer);
                            cacheObj = SerializationManager.DeserializeBin<CacheObject<T>>(buffer);
                        }
                    }
                }
                catch (ThreadInterruptedException ex) { }
                catch (ThreadAbortException ex)
                {
                    Thread.ResetAbort();
                }
                catch (IOException ex) { }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
                }
            }

            return cacheObj;
        }

        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="internalObject"></param>
        /// <param name="timeout"></param>
        private static void InsertCache(LocalCacheType type, string path, string key, T internalObject, TimeSpan timeout)
        {
            var cacheObj = new CacheObject<T>
            {
                Value = internalObject,
                ExpiredTime = DateTime.Now.Add(timeout)
            };

            if (type == LocalCacheType.File)
            {
                try
                {
                    lock (hashtable.SyncRoot)
                    {
                        var dir = Path.GetDirectoryName(path);

                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        var buffer = SerializationManager.SerializeBin(cacheObj);
                        buffer = CompressionManager.CompressGZip(buffer);
                        File.WriteAllBytes(path, buffer);

                        //默认缓存30秒
                        CacheHelper.Insert(key, cacheObj, (int)Math.Min(30, timeout.TotalSeconds));
                    }
                }
                catch (ThreadInterruptedException ex) { }
                catch (ThreadAbortException ex)
                {
                    Thread.ResetAbort();
                }
                catch (IOException ex) { }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
                }
            }
            else
            {
                //默认缓存1天
                CacheHelper.Insert(key, cacheObj, (int)TimeSpan.FromDays(1).TotalSeconds);
            }
        }

        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetFilePath(string key)
        {
            var cacheKey = MD5.HexHash(Encoding.Default.GetBytes(key));
            return CoreHelper.GetFullPath(string.Format("LocalCache\\{0}.dat", cacheKey));
        }
    }

    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum LocalCacheType
    {
        /// <summary>
        /// 内存方式
        /// </summary>
        Memory,
        /// <summary>
        /// 文件方式
        /// </summary>
        File
    }
}
