using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using MySoft.Logger;
using MySoft.Security;

namespace MySoft.IoC
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    public static class ServiceCacheHelper<T>
    {
        /// <summary>
        /// Hashtable.
        /// </summary>
        private static readonly Hashtable hashtable = new Hashtable();

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T Get(string serviceName, string methodName, string key, TimeSpan timeout, Func<T> func)
        {
            return Get(serviceName, methodName, key, timeout, func, null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static T Get(string serviceName, string methodName, string key, TimeSpan timeout, Func<T> func, Predicate<T> pred)
        {
            return Get(serviceName, methodName, key, timeout, state => func(), null, pred);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static T Get(string serviceName, string methodName, string key, TimeSpan timeout, Func<object, T> func, object state)
        {
            return Get(serviceName, methodName, key, timeout, func, state, null);
        }

        /// <summary>
        /// （本方法仅适应于本地缓存）
        /// 从缓存中获取数据，如获取失败，返回从指定的方法中获取
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static T Get(string serviceName, string methodName, string key, TimeSpan timeout, Func<object, T> func, object state, Predicate<T> pred)
        {
            var cacheObj = GetCache(serviceName, methodName, key, timeout);

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
                        cacheObj = InsertCache(serviceName, methodName, key, internalObject, timeout);
                    }
                    else
                    {
                        return internalObject;
                    }
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
                            hashtable[key] = new ArrayList { serviceName, methodName, timeout, func, pred };

                            //异步更新
                            func.BeginInvoke(state, AsyncCallback, key);
                        }
                    }
                }
            }

            if (cacheObj == null) return default(T);

            return cacheObj.Value;
        }

        /// <summary>
        /// 缓存回调
        /// </summary>
        /// <param name="ar"></param>
        private static void AsyncCallback(IAsyncResult ar)
        {
            var key = Convert.ToString(ar.AsyncState);

            try
            {
                var arr = hashtable[key] as ArrayList;
                var serviceName = Convert.ToString(arr[0]);
                var methodName = Convert.ToString(arr[1]);
                var timeout = (TimeSpan)arr[2];
                var func = arr[3] as Func<object, T>;
                var pred = arr[4] as Predicate<T>;

                var internalObject = func.EndInvoke(ar);

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
                        InsertCache(serviceName, methodName, key, internalObject, timeout);
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
                lock (hashtable)
                {
                    hashtable.Remove(key);
                }

                ar.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheObject<T> GetCache(string serviceName, string methodName, string key, TimeSpan timeout)
        {
            var cacheObj = CacheHelper.Get(key);
            if (cacheObj == null)
            {
                try
                {
                    var cacheKey = MD5.HexHash(Encoding.Default.GetBytes(key));
                    var path = CoreHelper.GetFullPath(string.Format("ServiceCache\\{0}\\{1}\\{2}.dat", serviceName, methodName, cacheKey));

                    if (File.Exists(path))
                    {
                        var buffer = File.ReadAllBytes(path);
                        buffer = CompressionManager.DecompressGZip(buffer);
                        cacheObj = SerializationManager.DeserializeBin(buffer);

                        //默认缓存5秒
                        CacheHelper.Insert(key, cacheObj, (int)Math.Min(5, timeout.TotalSeconds));
                    }
                }
                catch (IOException ex) { }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
                }
            }

            return cacheObj as CacheObject<T>;
        }

        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="key"></param>
        /// <param name="internalObject"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static CacheObject<T> InsertCache(string serviceName, string methodName, string key, T internalObject, TimeSpan timeout)
        {
            var cacheObj = new CacheObject<T>
            {
                Value = internalObject,
                ExpiredTime = DateTime.Now.Add(timeout)
            };

            try
            {
                var cacheKey = MD5.HexHash(Encoding.Default.GetBytes(key));
                var path = CoreHelper.GetFullPath(string.Format("ServiceCache\\{0}\\{1}\\{2}.dat", serviceName, methodName, cacheKey));

                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var buffer = SerializationManager.SerializeBin(cacheObj);
                buffer = CompressionManager.CompressGZip(buffer);
                File.WriteAllBytes(path, buffer);
            }
            catch (IOException ex) { }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("CacheHelper", ex);
            }

            return cacheObj;
        }
    }
}
