using System;
using System.Collections;
using System.Linq;
using System.Threading;

namespace MySoft.Cache
{
    /// <summary>
    /// QueueCache处理
    /// </summary>
    public class QueueCache
    {
        private ICacheStrategy cache;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化SessionCache
        /// </summary>
        /// <param name="cache"></param>
        public QueueCache(ICacheStrategy cache)
        {
            this.cache = cache;

            //启动线程存储缓存
            ThreadPool.QueueUserWorkItem(SaveCache);
        }

        /// <summary>
        /// 保存Queue数据
        /// </summary>
        /// <param name="state"></param>
        private void SaveCache(object state)
        {
            while (true)
            {
                if (hashtable.Count > 0)
                {
                    //获取CacheKey
                    var cacheKey = string.Empty;

                    try { cacheKey = hashtable.Keys.Cast<string>().FirstOrDefault(); }
                    catch { }

                    if (string.IsNullOrEmpty(cacheKey)) continue;

                    try
                    {
                        var item = hashtable[cacheKey] as QueueItem;

                        //保存值
                        if (item != null && cache != null)
                        {
                            try { cache.AddObject(item.Key, item.Value, DateTime.Now.Add(item.TimeSpan)); }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO
                    }
                    finally
                    {
                        hashtable.Remove(cacheKey);
                    }
                }

                //暂停10毫秒
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public object Get(string cacheKey)
        {
            if (hashtable.ContainsKey(cacheKey))
            {
                var item = hashtable[cacheKey] as QueueItem;
                return item.Value;
            }

            return cache.GetObject(cacheKey);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public T Get<T>(string cacheKey)
        {
            return (T)Get(cacheKey);
        }

        /// <summary>
        /// 存储key及value
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="timeSpan"></param>
        public void Add(string cacheKey, object cacheValue, TimeSpan timeSpan)
        {
            hashtable[cacheKey] = new QueueItem
            {
                Key = cacheKey,
                Value = cacheValue,
                TimeSpan = timeSpan
            };
        }

        /// <summary>
        /// 移除指定Key
        /// </summary>
        /// <param name="cacheKey"></param>
        public void Remove(string cacheKey)
        {
            if (hashtable.ContainsKey(cacheKey))
            {
                hashtable.Remove(cacheKey);
            }

            cache.RemoveObject(cacheKey);
        }

        /// <summary>
        /// QueueItem对象
        /// </summary>
        internal class QueueItem
        {
            /// <summary>
            /// Key
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Value
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// 时间
            /// </summary>
            public TimeSpan TimeSpan { get; set; }
        }
    }
}
