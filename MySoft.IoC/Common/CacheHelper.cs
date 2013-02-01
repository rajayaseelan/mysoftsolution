using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    internal static class ServiceCacheHelper
    {
        private const int UPDATE_TIMEOUT = 30;
        private static readonly Hashtable hashtable = new Hashtable();

        /// <summary>
        /// 从文件读取对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CacheItem GetCache(string filePath)
        {
            var key = Path.GetFileNameWithoutExtension(filePath);

            //从文件读取对象
            return GetCache(filePath, key);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static ResponseItem Get(CacheKey key, TimeSpan timeout, Func<ResponseItem> func)
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
        /// <param name="pred"></param>
        /// <returns></returns>
        public static ResponseItem Get(CacheKey key, TimeSpan timeout, Func<object, ResponseItem> func, object state)
        {
            var syncRoot = GetSyncRoot(key);

            lock (syncRoot)
            {
                ResponseItem item = null;
                var cacheObj = GetCache(GetFilePath(key), key.UniqueId);

                if (cacheObj == null)
                {
                    //获取数据缓存
                    item = GetResponseItem(key, timeout, func, state, false);
                }
                else if (cacheObj.ExpiredTime < DateTime.Now)
                {
                    //如果数据过期，则更新之
                    item = GetResponseItem(key, timeout, func, state, true);
                }

                if (item == null && cacheObj != null)
                {
                    //实例化Item
                    item = new ResponseItem { Buffer = cacheObj.Value, Count = cacheObj.Count };
                }

                return item;
            }
        }

        /// <summary>
        /// 获取锁对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static object GetSyncRoot(CacheKey key)
        {
            object syncRoot = null;

            string lockKey = string.Format("{0}${1}", key.ServiceName, key.MethodName);

            lock (hashtable.SyncRoot)
            {
                if (hashtable.ContainsKey(lockKey))
                {
                    syncRoot = hashtable[lockKey];
                }
                else
                {
                    syncRoot = new object();
                    hashtable[lockKey] = syncRoot;
                }
            }

            return syncRoot;
        }

        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="async"></param>
        /// <returns></returns>
        private static ResponseItem GetResponseItem(CacheKey key, TimeSpan timeout, Func<object, ResponseItem> func, object state, bool async)
        {
            ResponseItem item = null;

            try
            {
                if (async)
                {
                    var ar = func.BeginInvoke(state, null, null);

                    //等待30秒超时
                    if (ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(UPDATE_TIMEOUT)))
                    {
                        item = func.EndInvoke(ar);

                        ar.AsyncWaitHandle.Close();
                    }
                }
                else
                {
                    item = func(state);
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
                if (item != null && item.Buffer != null)
                {
                    //插入缓存
                    InsertCache(GetFilePath(key), key.UniqueId, item, timeout);
                }
            }

            return item;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static CacheItem GetCache(string path, string key)
        {
            var cacheObj = CacheHelper.Get<CacheItem>(key);

            if (cacheObj == null)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var buffer = File.ReadAllBytes(path);
                        cacheObj = SerializationManager.DeserializeBin<CacheItem>(buffer);
                    }
                }
                catch (ThreadInterruptedException ex) { }
                catch (ThreadAbortException ex)
                {
                    Thread.ResetAbort();
                }
                catch (SerializationException ex)
                {
                    try { File.Delete(path); }
                    catch { }
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
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="timeout"></param>
        private static void InsertCache(string path, string key, ResponseItem item, TimeSpan timeout)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);

                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                //序列化成二进制
                var cacheObj = new CacheItem
                {
                    ExpiredTime = DateTime.Now.Add(timeout),
                    Count = item.Count,
                    Value = item.Buffer
                };

                File.WriteAllBytes(path, SerializationManager.SerializeBin(cacheObj));

                //默认缓存30秒
                CacheHelper.Insert(key, cacheObj, (int)Math.Min(30, timeout.TotalSeconds));
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

        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetFilePath(CacheKey key)
        {
            return CoreHelper.GetFullPath(string.Format("ServiceCache\\{0}\\{1}\\{2}.dat",
                                            key.ServiceName, key.MethodName, key.UniqueId));
        }
    }
}