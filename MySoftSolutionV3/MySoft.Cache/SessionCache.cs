using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MySoft.Cache
{
    /// <summary>
    /// SessionCache处理
    /// </summary>
    public class SessionCache
    {
        private ICacheStrategy cache;
        private Queue<QueueData> queue;

        /// <summary>
        /// 实例化SessionCache
        /// </summary>
        /// <param name="cache"></param>
        public SessionCache(ICacheStrategy cache)
        {
            this.cache = cache;
            this.queue = new Queue<QueueData>();

            ThreadPool.QueueUserWorkItem(SaveQueueData);
        }

        /// <summary>
        /// 保存Queue数据
        /// </summary>
        /// <param name="state"></param>
        private void SaveQueueData(object state)
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    QueueData data = null;
                    lock (queue)
                    {
                        data = queue.Dequeue();
                    }

                    //保存值
                    if (data != null && cache != null)
                    {
                        try { cache.AddObject(data.Key, data.Value, data.TimeSpan); }
                        catch { }
                    }
                }

                //暂停100毫秒
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public T Get<T>(string cacheKey) where T : class
        {
            T obj = default(T);

            if (queue.Any(p => p.Key == cacheKey))
            {
                lock (queue)
                {
                    if (queue.Any(p => p.Key == cacheKey))
                    {
                        var value = queue.SingleOrDefault(p => p.Key == cacheKey);
                        if (value != null)
                        {
                            obj = (T)value.Value;
                        }
                    }
                }
            }

            if (obj == null)
            {
                try { obj = cache.GetObject<T>(cacheKey); }
                catch { }
            }

            return obj;
        }

        /// <summary>
        /// 存储key及value
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="timeSpan"></param>
        public void Add(string cacheKey, object cacheValue, TimeSpan timeSpan)
        {
            var data = new QueueData
            {
                Key = cacheKey,
                Value = cacheValue,
                TimeSpan = timeSpan
            };

            if (!queue.Any(p => p.Key == cacheKey))
            {
                lock (queue)
                {
                    //如果key存在，则不保存
                    if (!queue.Any(p => p.Key == cacheKey))
                    {
                        queue.Enqueue(data);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Queue数据
    /// </summary>
    internal class QueueData
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
