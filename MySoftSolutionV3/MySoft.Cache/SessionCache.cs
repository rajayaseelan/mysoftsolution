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
        private Queue<QueueTimeSpan> queue;

        /// <summary>
        /// 实例化SessionCache
        /// </summary>
        /// <param name="cache"></param>
        public SessionCache(ICacheStrategy cache)
        {
            this.cache = cache;
            this.queue = new Queue<QueueTimeSpan>();

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
                if (queue.Count > 0)
                {
                    QueueTimeSpan data = null;
                    lock (queue)
                    {
                        data = queue.Dequeue();
                    }

                    //保存值
                    if (data != null && cache != null)
                    {
                        try { cache.SetExpired(data.Key, DateTime.Now.Add(data.TimeSpan)); }
                        catch { }
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
            var obj = cache.GetObject(cacheKey);

            //处理缓存
            if (obj != null)
            {
                //获取过期时间
                var timeSpanKey = string.Format("SessionCache_{0}", cacheKey);
                var timeSpan = CacheHelper.Get(timeSpanKey);

                if (timeSpan != null)
                {
                    lock (queue)
                    {
                        //如果key存在，则不保存
                        if (!queue.Any(p => p.Key == cacheKey))
                        {
                            var data = new QueueTimeSpan
                            {
                                Key = cacheKey,
                                TimeSpan = (TimeSpan)timeSpan
                            };

                            queue.Enqueue(data);
                        }
                    }
                }
            }

            return obj;
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
            //存入缓存
            cache.AddObject(cacheKey, cacheValue, timeSpan);

            //记录过期时间
            var timeSpanKey = string.Format("SessionCache_{0}", cacheKey);
            CacheHelper.Permanent(timeSpanKey, timeSpan);
        }
    }

    /// <summary>
    /// Queue过期时间
    /// </summary>
    internal class QueueTimeSpan
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public TimeSpan TimeSpan { get; set; }
    }
}
