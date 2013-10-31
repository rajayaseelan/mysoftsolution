using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
using MySoft.Logger;

namespace MySoft.Cache
{
    /// <summary>
    /// 内存缓存管理类
    /// </summary>
    public class LocalCacheStrategy : CacheStrategyBase, ILocalCacheStrategy
    {
        /// <summary>
        /// 移除对象
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            lock (lockObject)
            {
                if (key == null || key.Length == 0)
                {
                    return;
                }

                webCache.Remove(key);
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                foreach (string key in AllKeys)
                {
                    webCache.Remove(key);
                }
            }
        }

        /// <summary>
        /// 获取所有Key值
        /// </summary>
        /// <returns></returns>
        public static IList<string> AllKeys
        {
            get
            {
                lock (lockObject)
                {
                    IDictionaryEnumerator cacheEnum = webCache.GetEnumerator();
                    IList<string> keys = new List<string>();

                    while (cacheEnum.MoveNext())
                    {
                        keys.Add(cacheEnum.Key.ToString());
                    }

                    return keys;
                }
            }
        }

        /// <summary>
        /// 内存缓存单例
        /// </summary>
        public static readonly LocalCacheStrategy Default = new LocalCacheStrategy(null);

        private static volatile System.Web.Caching.Cache webCache = System.Web.HttpRuntime.Cache;
        private static readonly object lockObject = new object();

        /// <summary>
        /// 实例化本地缓存
        /// </summary>
        /// <param name="bucketName"></param>
        public LocalCacheStrategy(string bucketName) : base(bucketName) { }

        /// <summary>
        /// 缓存对象
        /// </summary>
        public static System.Web.Caching.Cache CacheObject
        {
            get { return webCache; }
        }

        /// <summary>
        /// 缓存对象数
        /// </summary>
        public static int CacheCount
        {
            get { return AllKeys.Count; }
        }

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        public override void SetExpired(string key, DateTime datetime)
        {
            if (key == null || key.Length == 0)
            {
                return;
            }

            lock (lockObject)
            {
                //重新加入到缓存中
                var value = GetObject(key);
                if (value == null) return;

                AddObject(key, value, datetime);
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public override void AddObject(string key, object o)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                if (Timeout <= 0)
                {
                    webCache.Insert(GetInputKey(key), o, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
                else
                {
                    webCache.Insert(GetInputKey(key), o, null, DateTime.Now.AddSeconds(Timeout), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public override void AddObject(string key, object o, TimeSpan expires)
        {
            AddObject(key, o, DateTime.Now.Add(expires));
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public override void AddObject(string key, object o, DateTime datetime)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                webCache.Insert(GetInputKey(key), o, null, datetime, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并对相关文件建立依赖
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="files">监视的路径文件</param>
        public void AddObjectWithFileChange(string key, object o, string[] files)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(files, DateTime.Now);

                if (Timeout <= 0)
                {
                    webCache.Insert(GetInputKey(key), o, dep, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
                else
                {
                    webCache.Insert(GetInputKey(key), o, dep, System.DateTime.Now.AddSeconds(Timeout), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
            }
        }


        /// <summary>
        /// 加入当前对象到缓存中,并使用依赖键
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="dependKey">依赖关联的键值</param>
        public void AddObjectWithDepend(string key, object o, string[] dependKey)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(null, dependKey, DateTime.Now);

                if (Timeout <= 0)
                {
                    webCache.Insert(GetInputKey(key), o, dep, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
                else
                {
                    webCache.Insert(GetInputKey(key), o, dep, System.DateTime.Now.AddSeconds(Timeout), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并对相关文件建立依赖
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="files">监视的路径文件</param>
        public void AddObjectWithFileChange(string key, object o, TimeSpan expires, string[] files)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(files, DateTime.Now);

                webCache.Insert(GetInputKey(key), o, dep, System.DateTime.Now.Add(expires), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
            }
        }


        /// <summary>
        /// 加入当前对象到缓存中,并使用依赖键
        /// </summary>
        /// <param name="key">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="dependKey">依赖关联的键值</param>
        public void AddObjectWithDepend(string key, object o, TimeSpan expires, string[] dependKey)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(null, dependKey, DateTime.Now);

                webCache.Insert(GetInputKey(key), o, dep, System.DateTime.Now.Add(expires), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
            }
        }

        /// <summary>
        /// 建立回调委托的一个实例
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="reason"></param>
        public void onRemove(string key, object val, CacheItemRemovedReason reason)
        {
            //移除缓存事件
            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    break;
                case CacheItemRemovedReason.Expired:
                    break;
                case CacheItemRemovedReason.Removed:
                    break;
                case CacheItemRemovedReason.Underused:
                    break;
                default: break;
            }

            //如需要使用缓存日志,则需要使用下面代码
            //myLogVisitor.WriteLog(this,key,val,reason);

            //SimpleLog.Instance.WriteLogForDir("Cache", reason.ToString() + "：" + key);
            //MemoryManager.FlushMemory();
        }

        #region ICacheStrategy 成员

        /// <summary>
        /// 删除缓存对象
        /// </summary>
        /// <param name="key">对象的关键字</param>
        public override void RemoveObject(string key)
        {
            if (key == null || key.Length == 0)
            {
                return;
            }

            lock (lockObject)
            {
                webCache.Remove(GetInputKey(key));
            }
        }

        /// <summary>
        /// 返回一个指定的对象
        /// </summary>
        /// <param name="key">对象的关键字</param>
        /// <returns>对象</returns>
        public override object GetObject(string key)
        {
            if (key == null || key.Length == 0)
            {
                return null;
            }

            lock (lockObject)
            {
                return webCache.Get(GetInputKey(key));
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override T GetObject<T>(string key)
        {
            if (key == null || key.Length == 0)
            {
                return default(T);
            }

            lock (lockObject)
            {
                return (T)GetObject(GetInputKey(key));
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override object GetMatchObject(string regularExpression)
        {
            lock (lockObject)
            {
                IDictionary<string, object> values = GetMatchObjects(regularExpression);
                return values.Count > 0 ? values.First().Value : null;
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override T GetMatchObject<T>(string regularExpression)
        {
            lock (lockObject)
            {
                IDictionary<string, T> values = GetMatchObjects<T>(regularExpression);
                return values.Count > 0 ? values.First().Value : default(T);
            }
        }

        /// <summary>
        /// 移除所有缓存对象
        /// </summary>
        public override void RemoveAllObjects()
        {
            lock (lockObject)
            {
                IList<string> allKeys = GetAllKeys();
                RemoveObjects(allKeys);
            }
        }

        /// <summary>
        /// 获取所有的Key值
        /// </summary>
        /// <returns></returns>
        public override IList<string> GetAllKeys()
        {
            lock (lockObject)
            {
                IDictionaryEnumerator cacheEnum = webCache.GetEnumerator();
                List<string> keys = new List<string>();

                while (cacheEnum.MoveNext())
                {
                    keys.Add(cacheEnum.Key.ToString());
                }

                keys.RemoveAll(key => !key.StartsWith(prefix));
                return keys.ConvertAll<string>(key => GetOutputKey(key));
            }
        }

        /// <summary>
        /// 获取缓存数
        /// </summary>
        /// <returns></returns>
        public override int GetCacheCount()
        {
            lock (lockObject)
            {
                return GetAllKeys().Count;
            }
        }

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAllObjects()
        {
            lock (lockObject)
            {
                return GetObjects(GetAllKeys());
            }
        }

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override IDictionary<string, T> GetAllObjects<T>()
        {
            lock (lockObject)
            {
                return GetObjects<T>(GetAllKeys());
            }
        }

        /// <summary>
        /// 通过正则获取对应的Key列表
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public override IList<string> GetKeys(string regularExpression)
        {
            lock (lockObject)
            {
                if (regularExpression == null || regularExpression.Length == 0)
                {
                    return new List<string>();
                }

                IList<string> keys = new List<string>();
                Regex regex = new Regex(regularExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

                foreach (var key in GetAllKeys())
                {
                    if (regex.IsMatch(key)) keys.Add(key);
                }

                return keys;
            }
        }

        /// <summary>
        /// 添加多个对象到缓存
        /// </summary>
        /// <param name="data"></param>
        public override void AddObjects(IDictionary<string, object> data)
        {
            lock (lockObject)
            {
                foreach (KeyValuePair<string, object> kv in data)
                {
                    AddObject(GetInputKey(kv.Key), kv.Value);
                }
            }
        }

        /// <summary>
        /// 添加多个对象到缓存
        /// </summary>
        /// <param name="data"></param>
        public override void AddObjects<T>(IDictionary<string, T> data)
        {
            lock (lockObject)
            {
                foreach (KeyValuePair<string, T> kv in data)
                {
                    AddObject(GetInputKey(kv.Key), kv.Value);
                }
            }
        }

        /// <summary>
        /// 正则表达式方式移除对象
        /// </summary>
        /// <param name="regularExpression">匹配KEY正则表示式</param>
        public override void RemoveMatchObjects(string regularExpression)
        {
            lock (lockObject)
            {
                var keys = GetKeys(regularExpression);
                RemoveObjects(keys);
            }
        }

        /// <summary>
        /// 移除多个对象
        /// </summary>
        /// <param name="keys"></param>
        public override void RemoveObjects(IList<string> keys)
        {
            lock (lockObject)
            {
                foreach (string key in keys)
                {
                    RemoveObject(GetInputKey(key));
                }
            }
        }

        /// <summary>
        /// 获取多个对象
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override IDictionary<string, object> GetObjects(IList<string> keys)
        {
            lock (lockObject)
            {
                IDictionary<string, object> cacheData = new Dictionary<string, object>();
                foreach (string key in keys)
                {
                    var data = GetObject(GetInputKey(key));
                    if (data != null)
                        cacheData.Add(key, data);
                    else
                        cacheData.Add(key, null);
                }

                return cacheData;
            }
        }

        /// <summary>
        /// 获取多个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetObjects<T>(IList<string> keys)
        {
            lock (lockObject)
            {
                IDictionary<string, T> cacheData = new Dictionary<string, T>();
                foreach (string key in keys)
                {
                    var data = GetObject<T>(GetInputKey(key));
                    if (data != null)
                        cacheData.Add(key, data);
                    else
                        cacheData.Add(key, default(T));
                }

                return cacheData;
            }
        }

        /// <summary>
        /// 返回指定正则表达式的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public override IDictionary<string, object> GetMatchObjects(string regularExpression)
        {
            lock (lockObject)
            {
                return GetObjects(GetKeys(regularExpression));
            }
        }

        /// <summary>
        /// 返回指定正则表达的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetMatchObjects<T>(string regularExpression)
        {
            lock (lockObject)
            {
                return GetObjects<T>(GetKeys(regularExpression));
            }
        }

        #endregion
    }
}
