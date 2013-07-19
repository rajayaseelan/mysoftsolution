using MySoft.Logger;
using MySoft.Security;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;

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
            T internalObject = default(T);

            lock (GetSyncRoot(key))
            {
                //从内存获取
                internalObject = CacheHelper.Get<T>(key);

                //判断是否过期
                if (internalObject == null)
                {
                    //获取更新对象
                    if (type == LocalCacheType.Memory)
                        internalObject = SyncGetUpdateObject(type, key, timeout, func, state, pred);
                    else
                        internalObject = AsyncGetUpdateObject(type, key, timeout, func, state, pred);
                }
            }

            return internalObject;
        }

        /// <summary>
        /// 异步获取缓存对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        private static T AsyncGetUpdateObject(LocalCacheType type, string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
#if DEBUG
                Console.WriteLine("[{0}][{1}][{2}][{3}]", DateTime.Now, type, timeout, key);
#endif

            T internalObject = default(T);

            //从文件获取缓存
            var cacheObject = GetCache(GetFilePath(key));

            if (cacheObject == null)
            {
                internalObject = SyncGetUpdateObject(type, key, timeout, func, state, pred);
            }
            else
            {
                internalObject = cacheObject.Value;

                //更新默认缓存
                CacheHelper.Insert(key, internalObject, (int)timeout.TotalSeconds);

                if (cacheObject.ExpiredTime < DateTime.Now)
                {
                    func.BeginInvoke(state, AsyncUpdateObject, new ArrayList { type, key, timeout, pred });
                }
            }

            return internalObject;
        }

        /// <summary>
        /// 异步更新缓存
        /// </summary>
        /// <param name="ar"></param>
        private static void AsyncUpdateObject(IAsyncResult ar)
        {
            var arr = ar.AsyncState as ArrayList;

            try
            {
                var @delegate = (ar as AsyncResult).AsyncDelegate;
                var func = @delegate as Func<object, T>;
                var type = (LocalCacheType)arr[0];
                var key = Convert.ToString(arr[1]);
                var timeout = (TimeSpan)arr[2];
                var pred = arr[3] as Predicate<T>;

                var internalObject = func.EndInvoke(ar);

                //更新缓存项
                if (CheckUpdateResult(internalObject, pred))
                {
                    //插入内存
                    InsertCacheObject(type, key, internalObject, timeout);
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

        #endregion

        #region 私有方法

        /// <summary>
        /// 同步获取缓存对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        private static T SyncGetUpdateObject(LocalCacheType type, string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
#if DEBUG
                Console.WriteLine("[{0}][{1}][{2}][{3}]", DateTime.Now, type, timeout, key);
#endif

            var internalObject = func(state);

            //更新缓存项
            if (CheckUpdateResult(internalObject, pred))
            {
                //插入内存
                InsertCacheObject(type, key, internalObject, timeout);
            }

            return internalObject;
        }

        /// <summary>
        /// 检测更新结果
        /// </summary>
        /// <param name="internalObject"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        private static bool CheckUpdateResult(T internalObject, Predicate<T> pred)
        {
            if (internalObject == null) return false;

            var success = true;

            if (pred != null)
            {
                try
                {
                    if (internalObject != null)
                    {
                        success = pred(internalObject);
                    }
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        #region 文件缓存处理

        /// <summary>
        /// 插入文件缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="internalObject"></param>
        /// <param name="timeout"></param>
        private static void InsertCacheObject(LocalCacheType type, string key, T internalObject, TimeSpan timeout)
        {
            lock (GetSyncRoot(key))
            {
                //插入内存
                CacheHelper.Insert(key, internalObject, (int)timeout.TotalSeconds);

                if (type == LocalCacheType.File)
                {
                    try
                    {
                        var cacheObj = new CacheObject<T>
                        {
                            Value = internalObject,
                            ExpiredTime = DateTime.Now.Add(timeout)
                        };

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
            }
        }

        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetFilePath(string key)
        {
            var arr = key.Split(new[] { "_$$_" }, StringSplitOptions.RemoveEmptyEntries);

            var fileName = arr.Last();
            var rootPath = string.Join("\\", arr.Where(p => p != fileName).ToArray());

            var filePath = Path.Combine(rootPath, MD5.HexHash(Encoding.Default.GetBytes(fileName)));

            return CoreHelper.GetFullPath(string.Format("LocalCache\\{0}.dat", filePath));
        }

        #endregion

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
