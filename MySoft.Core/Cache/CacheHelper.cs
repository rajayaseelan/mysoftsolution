using System;
using System.Collections;
using System.IO;
using System.Text;
using MySoft.Logger;
using MySoft.Security;

namespace MySoft.Cache
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    public static class CacheHelper<T>
    {
        #region 数据缓存

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
                var buffer = File.ReadAllBytes(filePath);
                buffer = CompressionManager.DecompressGZip(buffer);
                var cacheObj = SerializationManager.DeserializeBin<CacheObject<T>>(buffer);

                return cacheObj;
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
                    var path = GetFilePath(key);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    //移除缓存
                    CacheHelper.Remove(key);
                }
                catch (IOException ex) { }
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
            T cacheItem = default(T);

            lock (GetSyncRoot(key))
            {
                var cacheObj = GetCacheItem(type, GetFilePath(key), key, timeout);

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
                        //获取更新对象
                        cacheItem = GetUpdateObject(type, key, timeout, func, state, pred);
                    }

                    if (cacheItem == null)
                    {
                        cacheItem = cacheObj.Value;
                    }
                }
            }

            return cacheItem;
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
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
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
                    InsertCacheItem(type, GetFilePath(key), key, internalObject, timeout);
                }
            }

            return internalObject;
        }

        #endregion

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheObject<T> GetCacheItem(LocalCacheType type, string path, string key, TimeSpan timeout)
        {
            var cacheObj = CacheHelper.Get<CacheObject<T>>(key);

            if (type == LocalCacheType.File && cacheObj == null)
            {
                try
                {
                    cacheObj = GetCache(path);

                    if (cacheObj != null)
                    {
                        //默认缓存60秒
                        CacheHelper.Insert(key, cacheObj, Math.Min(60, (int)timeout.TotalSeconds));
                    }
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
        private static void InsertCacheItem(LocalCacheType type, string path, string key, T internalObject, TimeSpan timeout)
        {
            var cacheObj = new CacheObject<T>
            {
                Value = internalObject,
                ExpiredTime = DateTime.Now.Add(timeout)
            };

            if (type == LocalCacheType.File)
            {
                //默认缓存60秒
                CacheHelper.Insert(key, cacheObj, Math.Min(60, (int)timeout.TotalSeconds));

                try
                {
                    var buffer = SerializationManager.SerializeBin(cacheObj);
                    buffer = CompressionManager.CompressGZip(buffer);

                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(path, buffer);
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
