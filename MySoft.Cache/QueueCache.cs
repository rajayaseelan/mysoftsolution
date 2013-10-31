using System;
using System.Collections;

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

            //异步调用
            new Action<string>(AsyncRun).BeginInvoke(cacheKey, null, null);
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="key"></param>
        private void AsyncRun(string key)
        {
            if (!hashtable.ContainsKey(key)) return;

            try
            {
                var item = hashtable[key] as QueueItem;

                //保存值
                if (item != null && cache != null)
                {
                    try { cache.AddObject(item.Key, item.Value, DateTime.Now.Add(item.TimeSpan)); }
                    catch { }
                }
            }
            catch (Exception ex) { }
            finally
            {
                hashtable.Remove(key);
            }
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
