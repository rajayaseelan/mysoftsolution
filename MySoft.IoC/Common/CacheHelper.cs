using System;
using System.IO;
using System.Threading;
using MySoft.Logger;

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
            //从文件读取对象
            if (File.Exists(filePath))
            {
                var buffer = File.ReadAllBytes(filePath);
                var cacheObj = SerializationManager.DeserializeBin<CacheItem>(buffer);

                return cacheObj;
            }

            return null;
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
        /// <param name="cacheKey"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ResponseItem Get(CacheKey cacheKey, TimeSpan timeout, Func<object, ResponseItem> func, object state)
        {
            //定义缓存项
            ResponseItem cacheItem = null;

            var cacheObj = GetCacheItem(cacheKey, timeout);

            if (cacheObj == null)
            {
                //获取数据缓存
                cacheItem = func(state);

                if (cacheItem != null && cacheItem.Buffer != null)
                {
                    //插入缓存
                    InsertCacheItem(cacheKey, cacheItem, timeout);
                }
            }
            else
            {
                //判断是否过期
                if (cacheObj.ExpiredTime < DateTime.Now)
                {
                    //更新缓存项
                    UpdateCacheItem(cacheKey, func, state, timeout);
                }

                //获取数据缓存
                cacheItem = new ResponseItem { Buffer = cacheObj.Value, Count = cacheObj.Count };
            }

            //返回缓存的对象
            return cacheItem;
        }

        /// <summary>
        /// 更新缓存项
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        private static void UpdateCacheItem(CacheKey cacheKey, Func<object, ResponseItem> func, object state, TimeSpan timeout)
        {
            //获取数据缓存
            var updateItem = new CacheUpdateItem
            {
                Key = cacheKey,
                Func = func,
                State = state,
                Timeout = timeout
            };

            //异步更新缓存
            ThreadPool.QueueUserWorkItem(item =>
            {
                //获取数据缓存
                var cacheItem = func(state);

                if (cacheItem != null && cacheItem.Buffer != null)
                {
                    //插入缓存
                    InsertCacheItem(cacheKey, cacheItem, timeout);
                }

            }, updateItem);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheItem GetCacheItem(CacheKey cacheKey, TimeSpan timeout)
        {
            var key = cacheKey.UniqueId;
            var cacheObj = CacheHelper.Get<CacheItem>(key);

            if (cacheObj == null)
            {
                var path = GetFilePath(cacheKey);

                try
                {
                    //从文件获取缓存
                    cacheObj = GetCache(path);

                    if (cacheObj != null)
                    {
                        //默认缓存30秒
                        CacheHelper.Insert(key, cacheObj, Math.Min(30, (int)timeout.TotalSeconds));
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
        /// <param name="cacheKey"></param>
        /// <param name="item"></param>
        /// <param name="timeout"></param>
        private static void InsertCacheItem(CacheKey cacheKey, ResponseItem item, TimeSpan timeout)
        {
            var key = cacheKey.UniqueId;

            //序列化成二进制
            var cacheObj = new CacheItem
            {
                ExpiredTime = DateTime.Now.Add(timeout),
                Count = item.Count,
                Value = item.Buffer
            };

            //默认缓存30秒
            CacheHelper.Insert(key, cacheObj, Math.Min(30, (int)timeout.TotalSeconds));

            try
            {
                var path = GetFilePath(cacheKey);

                //写入文件
                var buffer = SerializationManager.SerializeBin(cacheObj);

                string dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                File.WriteAllBytes(path, buffer);
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
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        private static string GetFilePath(CacheKey cacheKey)
        {
            return CoreHelper.GetFullPath(string.Format("ServiceCache\\{0}\\{1}\\{2}.dat",
                                            cacheKey.ServiceName, cacheKey.MethodName, cacheKey.UniqueId));
        }
    }
}