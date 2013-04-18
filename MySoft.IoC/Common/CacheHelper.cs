using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using MySoft.Logger;
using MySoft.Threading;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    internal static class ServiceCacheHelper
    {
        /// <summary>
        /// 从文件读取对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CacheItem GetCache(string filePath)
        {
            var key = Path.GetFileNameWithoutExtension(filePath);

            //从文件读取对象
            return GetCacheItem(filePath, key);
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
            ResponseItem cacheItem = null;

            var cacheObj = GetCacheItem(GetFilePath(key), key.UniqueId);

            if (cacheObj == null)
            {
                //获取数据缓存
                cacheItem = GetResponseItem(key, timeout, func, state);
            }
            else
            {
                if (cacheObj.ExpiredTime < DateTime.Now)
                {
                    //获取数据缓存
                    cacheItem = GetResponseItem(key, timeout, func, state);
                }
                else
                {
                    //获取数据缓存
                    cacheItem = new ResponseItem { Buffer = cacheObj.Value, Count = cacheObj.Count };
                }
            }

            //返回缓存的对象
            return cacheItem;
        }

        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private static ResponseItem GetResponseItem(CacheKey key, TimeSpan timeout, Func<object, ResponseItem> func, object state)
        {
            ResponseItem item = null;

            try
            {
                item = func(state);
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
                    InsertCacheItem(GetFilePath(key), key.UniqueId, item, timeout);
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
        private static CacheItem GetCacheItem(string path, string key)
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
        private static void InsertCacheItem(string path, string key, ResponseItem item, TimeSpan timeout)
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