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
        private const int UPDATE_TIMEOUT = 30;

        /// <summary>
        /// 从文件读取对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CacheObject<byte[]> GetCache(string filePath)
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
            var cacheObj = GetCache(GetFilePath(key), key.UniqueId);

            if (cacheObj == null)
            {
                //获取数据缓存
                return GetUpdateBuffer(key, timeout, func, state, false);
            }
            else if (cacheObj.ExpiredTime < DateTime.Now)
            {
                //如果数据过期，则更新之
                var buffer = GetUpdateBuffer(key, timeout, func, state, true);

                if (buffer != null) return buffer;
            }

            return cacheObj.Value;
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
        private static byte[] GetUpdateBuffer(CacheKey key, TimeSpan timeout, Func<object, byte[]> func, object state, bool async)
        {
            byte[] buffer = null;

            try
            {
                if (async)
                {
                    var ar = func.BeginInvoke(state, null, null);

                    //等待30秒超时
                    if (ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(UPDATE_TIMEOUT)))
                    {
                        buffer = func.EndInvoke(ar);

                        ar.AsyncWaitHandle.Close();
                    }
                }
                else
                {
                    buffer = func(state);
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
                if (buffer != null)
                {
                    //插入缓存
                    InsertCache(GetFilePath(key), key.UniqueId, buffer, timeout);
                }
            }

            return buffer;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static CacheObject<byte[]> GetCache(string path, string key)
        {
            var cacheObj = CacheHelper.Get<CacheObject<byte[]>>(key);

            if (cacheObj == null)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var buffer = File.ReadAllBytes(path);
                        cacheObj = SerializationManager.DeserializeBin<CacheObject<byte[]>>(buffer);
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
                var dir = Path.GetDirectoryName(path);

                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                //序列化成二进制
                var cacheObj = new CacheObject<byte[]>
                {
                    ExpiredTime = DateTime.Now.Add(timeout),
                    Value = buffer
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