using System;
using System.Collections;
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
        /// Hashtable.
        /// </summary>
        private static readonly Hashtable hashtable = new Hashtable();

        /// <summary>
        /// 从文件读取对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CacheObject<byte[]> GetCache(string filePath)
        {
            var key = Path.GetFileNameWithoutExtension(filePath);

            //从文件读取对象
            return GetCache(filePath, key, TimeSpan.MaxValue);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static byte[] Get(CacheKey key, TimeSpan timeout, Func<byte[]> func)
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
        public static byte[] Get(CacheKey key, TimeSpan timeout, Func<object, byte[]> func, object state)
        {
            var cacheObj = GetCache(GetFilePath(key), key.UniqueId, timeout);

            if (cacheObj == null)
            {
                byte[] buffer = null;

                try
                {
                    buffer = func(state);
                }
                catch (ThreadInterruptedException ex) { }
                catch (ThreadAbortException ex)
                {
                    Thread.ResetAbort();
                }

                if (buffer != null)
                {
                    buffer = CompressionManager.CompressGZip(buffer);

                    //插入缓存
                    InsertCache(GetFilePath(key), key.UniqueId, buffer, timeout);
                }

                return buffer;
            }
            else
            {
                //如果数据过期，则更新之
                if (cacheObj.ExpiredTime < DateTime.Now)
                {
                    lock (hashtable.SyncRoot)
                    {
                        if (!hashtable.ContainsKey(key.UniqueId))
                        {
                            hashtable[key.UniqueId] = new ArrayList { timeout, func };

                            //异步更新
                            func.BeginInvoke(state, AsyncCallback, key);
                        }
                    }
                }

                return cacheObj.Value;
            }
        }

        /// <summary>
        /// 缓存回调
        /// </summary>
        /// <param name="ar"></param>
        private static void AsyncCallback(IAsyncResult ar)
        {
            var key = ar.AsyncState as CacheKey;
            if (key == null) return;

            try
            {
                var arr = hashtable[key.UniqueId] as ArrayList;
                var timeout = (TimeSpan)arr[0];
                var func = arr[1] as Func<object, byte[]>;

                var buffer = func.EndInvoke(ar);

                if (buffer != null)
                {
                    buffer = CompressionManager.CompressGZip(buffer);

                    //插入缓存
                    InsertCache(GetFilePath(key), key.UniqueId, buffer, timeout);
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
                    hashtable.Remove(key.UniqueId);
                }

                ar.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheObject<byte[]> GetCache(string path, string key, TimeSpan timeout)
        {
            var cacheObj = CacheHelper.Get<CacheObject<byte[]>>(key);

            if (cacheObj == null)
            {
                try
                {
                    lock (hashtable.SyncRoot)
                    {
                        if (File.Exists(path))
                        {
                            var buffer = File.ReadAllBytes(path);
                            cacheObj = SerializationManager.DeserializeBin<CacheObject<byte[]>>(buffer);

                            if (timeout != TimeSpan.MaxValue)
                            {
                                //默认缓存5秒
                                CacheHelper.Insert(key, cacheObj, (int)Math.Min(5, timeout.TotalSeconds));
                            }
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
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="timeout"></param>
        private static void InsertCache(string path, string key, byte[] buffer, TimeSpan timeout)
        {
            try
            {
                lock (hashtable.SyncRoot)
                {
                    var dir = Path.GetDirectoryName(path);

                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    //序列化成二进制
                    var cacheObj = new CacheObject<byte[]>
                    {
                        ExpiredTime = DateTime.Now.Add(timeout),
                        Value = buffer
                    };

                    File.WriteAllBytes(path, SerializationManager.SerializeBin(cacheObj));
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