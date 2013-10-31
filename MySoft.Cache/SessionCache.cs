using System;
using System.Collections;

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
        }

        /// <summary>
        /// 获取值（默认保持20分钟）
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public object Get(string cacheKey)
        {
            return Get(cacheKey, TimeSpan.FromMinutes(20));
        }

        /// <summary>
        /// 获取值（指定保持时间）
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public object Get(string cacheKey, TimeSpan timeSpan)
        {
            //如果key存在，则不保存
            if (!hashtable.ContainsKey(cacheKey))
            {
                var value = cache.GetObject(cacheKey);

                if (value != null)
                {
                    var item = new SessionItem
                    {
                        Key = cacheKey,
                        TimeSpan = (TimeSpan)timeSpan,
                        Value = value
                    };

                    hashtable[cacheKey] = item;

                    //异步调用
                    new Action<string>(AsyncRun).BeginInvoke(cacheKey, null, null);
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
        /// 异步调用
        /// </summary>
        /// <param name="key"></param>
        private void AsyncRun(string key)
        {
            if (!hashtable.ContainsKey(key)) return;

            try
            {
                var item = hashtable[key] as SessionItem;

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
                hashtable.Remove(key);
            }
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
