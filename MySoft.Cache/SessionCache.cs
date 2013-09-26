using System;
using System.Collections;
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
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化SessionCache
        /// </summary>
        /// <param name="cache"></param>
        public SessionCache(ICacheStrategy cache)
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
                        var item = hashtable[cacheKey] as SessionItem;

                        //保存值
                        if (item != null && cache != null)
                        {
                            try { cache.SetExpired(item.Key, DateTime.Now.Add(item.TimeSpan)); }
                            catch { }
                        }
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        hashtable.Remove(cacheKey);
                    }
                }

                //暂停1秒
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public object Get(string cacheKey)
        {
            //如果key存在，则不保存
            if (!hashtable.ContainsKey(cacheKey))
            {
                var value = cache.GetObject(cacheKey);

                if (value != null)
                {
                    //获取过期时间
                    var timeSpanKey = string.Format("SessionCache_{0}", cacheKey);
                    var timeSpan = CacheHelper.Get(timeSpanKey);

                    if (timeSpan != null)
                    {
                        var item = new SessionItem
                        {
                            Key = cacheKey,
                            TimeSpan = (TimeSpan)timeSpan,
                            Value = value
                        };

                        hashtable[cacheKey] = item;
                    }
                }
            }

            //返回值
            if (hashtable.ContainsKey(cacheKey))
            {
                return (hashtable[cacheKey] as SessionItem).Value;
            }

            return null;
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
            CacheHelper.Insert(timeSpanKey, timeSpan, (int)TimeSpan.FromDays(1).TotalSeconds);
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

            var timeSpanKey = string.Format("SessionCache_{0}", cacheKey);
            CacheHelper.Remove(timeSpanKey);

            cache.RemoveObject(cacheKey);
        }

        /// <summary>
        /// SessionItem对象
        /// </summary>
        internal class SessionItem
        {
            /// <summary>
            /// Key
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// 时间
            /// </summary>
            public TimeSpan TimeSpan { get; set; }

            /// <summary>
            /// 数据
            /// </summary>
            public object Value { get; set; }
        }
    }
}
