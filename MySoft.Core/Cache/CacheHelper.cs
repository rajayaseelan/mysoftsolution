using MySoft.Logger;
using MySoft.Security;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;

namespace MySoft.Cache
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    public static class CacheHelper<T>
    {
        #region 默认内存缓存

        /// <summary>
        /// 从文件读取对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CacheObject<T> GetCache(string filePath)
        {
            //从文件读取对象
            if (File.Exists(filePath))
            {
                try
                {
                    var buffer = File.ReadAllBytes(filePath);
                    var cacheObj = SerializationManager.DeserializeBin<CacheObject<T>>(buffer);

                    return cacheObj;
                }
                catch (SerializationException ex)
                {
                    File.Delete(filePath);

                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            Remove(LocalCacheType.Memory, key);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        public static void Remove(LocalCacheType type, string key)
        {
            if (type == LocalCacheType.Memory)
            {
                //移除缓存
                CacheHelper.Remove(key);
            }
            else
            {
                try
                {
                    lock (GetSyncRoot(key))
                    {
                        var path = GetFilePath(key);

                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }

                    //移除缓存
                    CacheHelper.Remove(key);
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
                }
            }
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

        #region 支撑缓存类型

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
            T cacheItem = default(T);

            lock (GetSyncRoot(key))
            {
                //从内存获取
                var cacheObj = CacheHelper.Get<CacheObject<T>>(key);

                if (cacheObj == null && type == LocalCacheType.File)
                {
                    //从文件获取缓存
                    cacheObj = GetFileCacheItem(key, timeout);
                }

                if (cacheObj == null)
                {
                    //获取更新对象
                    cacheItem = GetUpdateObject(type, key, timeout, func, state, pred);
                }
                else
                {
                    //判断是否过期
                    if (cacheObj.ExpiredTime < DateTime.Now)
                    {
                        //延长过期时间
                        cacheObj.ExpiredTime = DateTime.Now.Add(timeout);

                        UpdateCacheAsync(type, key, timeout, func, state, pred);
                    }

                    cacheItem = cacheObj.Value;
                }
            }

            return cacheItem;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 异步更新缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        private static void UpdateCacheAsync(LocalCacheType type, string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
            func.BeginInvoke(state, ar =>
            {
                var arr = ar.AsyncState as ArrayList;

                try
                {
                    var _func = arr[0] as Func<object, T>;
                    var _type = (LocalCacheType)arr[1];
                    var _key = Convert.ToString(arr[2]);
                    var _timeout = (TimeSpan)arr[3];
                    var _pred = arr[4] as Predicate<T>;

                    var internalObject = _func.EndInvoke(ar);

#if DEBUG
                    Console.WriteLine("[{0}][{1}][{2}][{3}]", DateTime.Now, _type, _timeout, _key);
#endif

                    //更新缓存项
                    UpdateCacheSync(internalObject, _type, _key, _timeout, _pred);
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
                }
                finally
                {
                    //关闭句柄
                    ar.AsyncWaitHandle.Close();
                }
            }, new ArrayList { func, type, key, timeout, pred });
        }

        /// <summary>
        /// 获取更新对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        private static T GetUpdateObject(LocalCacheType type, string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
            T internalObject = default(T);

            try
            {
                internalObject = func(state);

                //更新缓存项
                UpdateCacheSync(internalObject, type, key, timeout, pred);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
            }

            return internalObject;
        }

        /// <summary>
        /// 更新缓存项
        /// </summary>
        /// <param name="internalObject"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="pred"></param>
        private static void UpdateCacheSync(T internalObject, LocalCacheType type, string key, TimeSpan timeout, Predicate<T> pred)
        {
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
                    InsertCacheItem(type, key, internalObject, timeout);
                }
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheObject<T> GetFileCacheItem(string key, TimeSpan timeout)
        {
            try
            {
                var cacheObj = GetCache(GetFilePath(key));

                if (cacheObj != null)
                {
                    //默认缓存60秒
                    CacheHelper.Insert(key, cacheObj, (int)timeout.TotalSeconds);
                }

                return cacheObj;
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
            }

            return null;
        }

        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="internalObject"></param>
        /// <param name="timeout"></param>
        private static void InsertCacheItem(LocalCacheType type, string key, T internalObject, TimeSpan timeout)
        {
            var cacheObj = new CacheObject<T>
            {
                Value = internalObject,
                ExpiredTime = DateTime.Now.Add(timeout)
            };

            if (type == LocalCacheType.File)
            {
                //插入内存
                CacheHelper.Insert(key, cacheObj, (int)timeout.TotalSeconds);

                try
                {
                    var buffer = SerializationManager.SerializeBin(cacheObj);

                    //获取文件路径
                    var filePath = GetFilePath(key);

                    var dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    File.WriteAllBytes(filePath, buffer);
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
                }
            }
            else
            {
                //插入内存
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
            var arr = key.Split('$');

            var fileName = arr.Last();
            var rootPath = string.Join("\\", arr.Where(p => p != fileName).ToArray());

            var filePath = Path.Combine(rootPath, MD5.HexHash(Encoding.Default.GetBytes(fileName)));

            return CoreHelper.GetFullPath(string.Format("LocalCache\\{0}.dat", filePath));
        }

        //读文件同步
        private static readonly Hashtable hashtable = new Hashtable();

        /// <summary>
        /// 获取同步Root
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static object GetSyncRoot(string key)
        {
            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(key))
                {
                    hashtable[key] = new object();
                }

                return hashtable[key];
            }
        }

        #endregion
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
