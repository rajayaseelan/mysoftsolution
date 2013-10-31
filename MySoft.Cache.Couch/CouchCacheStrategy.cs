using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Couchbase;
using Couchbase.Configuration;
using Enyim.Caching.Memcached;

namespace MySoft.Cache
{
    /// <summary>
    /// 分布式缓存管理类
    /// </summary>
    public class CouchCacheStrategy : CacheStrategyBase, IDistributedCacheStrategy
    {
        private ICacheStrategy localCache;
        private TimeSpan localTimeSpan;

        /// <summary>
        /// 设置本地缓存超时时间
        /// </summary>
        /// <param name="timeout">超时时间，单位：秒</param>
        public void SetLocalCacheTimeout(int timeout)
        {
            if (timeout > 0)
            {
                this.localCache = CacheFactory.Create("Local_" + base.bucketName, CacheType.Local);
                this.localCache.Timeout = timeout;
                this.localTimeSpan = TimeSpan.FromSeconds(timeout);
            }
            else
                this.localCache = null;
        }

        private static readonly object lockObject = new object();
        private CouchbaseClient dataCache;

        /// <summary>
        /// 实例化分布式缓存
        /// </summary>
        /// <param name="bucketName"></param>
        public CouchCacheStrategy(string bucketName)
            : base(bucketName)
        {
            //默认配置节名称为couchbase
            if (string.IsNullOrEmpty(bucketName))
            {
                bucketName = "couchbase";
            }

            var config = (ICouchbaseClientConfiguration)ConfigurationManager.GetSection(bucketName);
            if (config == null)
            {
                throw new KeyNotFoundException("未找到名称为 [" + bucketName + "] 的配置节！");
            }

            //实例化CouchbaseClient
            this.dataCache = new CouchbaseClient(config);
        }

        #region ICacheStrategy 成员

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
                dataCache.Touch(GetInputKey(key), datetime);
            }
        }

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        public override void AddObject(string key, object o)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                if (Timeout <= 0)
                {
                    dataCache.Store(StoreMode.Add, GetInputKey(key), o);
                }
                else
                {
                    dataCache.Store(StoreMode.Add, GetInputKey(key), o, DateTime.Now.AddSeconds(Timeout));
                }

                //处理本地缓存
                if (localCache != null) localCache.AddObject(key, o, localTimeSpan);
            }
        }

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        public override void AddObject(string key, object o, TimeSpan expires)
        {
            AddObject(key, o, DateTime.Now.Add(expires));
        }

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        public override void AddObject(string key, object o, DateTime datetime)
        {
            if (key == null || key.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                if (Timeout > 0)
                {
                    dataCache.Store(StoreMode.Add, GetInputKey(key), o, datetime);
                }
                else
                {
                    dataCache.Store(StoreMode.Add, GetInputKey(key), o);
                }

                //处理本地缓存
                if (localCache != null) localCache.AddObject(key, o, localTimeSpan);
            }
        }

        /// <summary>
        /// 移除指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        public override void RemoveObject(string key)
        {
            if (key == null || key.Length == 0)
            {
                return;
            }

            lock (lockObject)
            {
                dataCache.Remove(GetInputKey(key));

                //处理本地缓存
                if (localCache != null) localCache.RemoveObject(key);
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override object GetObject(string key)
        {
            if (key == null || key.Length == 0)
            {
                return null;
            }

            lock (lockObject)
            {
                object returnObject = null;

                //处理本地缓存
                if (localCache != null)
                {
                    returnObject = localCache.GetObject(key);
                    if (returnObject != null) return returnObject;
                }

                returnObject = dataCache.Get(GetInputKey(key));

                //添加到本地缓存
                if (returnObject != null && localCache != null)
                {
                    localCache.AddObject(key, returnObject, localTimeSpan);
                }

                return returnObject;
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override T GetObject<T>(string key)
        {
            return (T)GetObject(key);
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override object GetMatchObject(string regularExpression)
        {
            throw new NotSupportedException("不支持GetMatchObject方法！");
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override T GetMatchObject<T>(string regularExpression)
        {
            throw new NotSupportedException("不支持GetMatchObject方法！");
        }

        /// <summary>
        /// 移除所有缓存对象
        /// </summary>
        public override void RemoveAllObjects()
        {
            dataCache.FlushAll();
        }

        /// <summary>
        /// 获取所有Key值
        /// </summary>
        /// <returns></returns>
        public override IList<string> GetAllKeys()
        {
            throw new NotSupportedException("不支持GetAllKeys方法！");
        }

        /// <summary>
        /// 获取缓存数
        /// </summary>
        /// <returns></returns>
        public override int GetCacheCount()
        {
            throw new NotSupportedException("不支持GetCacheCount方法！");
        }

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAllObjects()
        {
            throw new NotSupportedException("不支持GetAllObjects方法！");
        }

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override IDictionary<string, T> GetAllObjects<T>()
        {
            throw new NotSupportedException("不支持GetAllObjects方法！");
        }

        /// <summary>
        /// 通过正则获取对应的Key列表
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public override IList<string> GetKeys(string regularExpression)
        {
            throw new NotSupportedException("不支持GetKeys方法！");
        }

        /// <summary>
        /// 添加多个对象
        /// </summary>
        /// <param name="data"></param>
        public override void AddObjects(IDictionary<string, object> data)
        {
            lock (lockObject)
            {
                foreach (KeyValuePair<string, object> kv in data)
                {
                    if (kv.Value != null)
                    {
                        AddObject(kv.Key, kv.Value);
                    }
                }

            }
        }

        /// <summary>
        /// 添加多个对象
        /// </summary>
        /// <param name="data"></param>
        public override void AddObjects<T>(IDictionary<string, T> data)
        {
            lock (lockObject)
            {
                foreach (KeyValuePair<string, T> kv in data)
                {
                    if (kv.Value != null)
                    {
                        AddObject(kv.Key, kv.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 正则表达式方式移除对象
        /// </summary>
        /// <param name="regularExpression">匹配KEY正则表示式</param>
        public override void RemoveMatchObjects(string regularExpression)
        {
            throw new NotSupportedException("不支持此方法！");
        }

        /// <summary>
        /// 移除多个对象
        /// </summary>
        /// <param name="keys"></param>
        public override void RemoveObjects(IList<string> keys)
        {
            lock (lockObject)
            {
                var objIdList = new List<string>(keys);
                objIdList = objIdList.ConvertAll<string>(key => GetInputKey(key));
                objIdList = (from item in objIdList select item).Distinct().ToList();

                foreach (var key in objIdList)
                {
                    dataCache.Remove(key);
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
                var objIdList = new List<string>(keys);
                objIdList = objIdList.ConvertAll<string>(key => GetInputKey(key));
                objIdList = (from item in objIdList select item).Distinct().ToList();

                var dictCache = dataCache.Get(objIdList);
                IDictionary<string, object> cacheData = new Dictionary<string, object>();
                foreach (var key in objIdList)
                {
                    if (dictCache.ContainsKey(key))
                        cacheData.Add(key, dictCache[key]);
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
                var objIdList = new List<string>(keys);
                objIdList = objIdList.ConvertAll<string>(key => GetInputKey(key));
                objIdList = (from item in objIdList select item).Distinct().ToList();

                var dictCache = dataCache.Get(objIdList);
                IDictionary<string, T> cacheData = new Dictionary<string, T>();
                foreach (var key in objIdList)
                {
                    if (dictCache.ContainsKey(key))
                        cacheData.Add(key, (T)dictCache[key]);
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
            throw new NotSupportedException("不支持GetMatchObjects方法！");
        }

        /// <summary>
        /// 返回指定正则表达的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetMatchObjects<T>(string regularExpression)
        {
            throw new NotSupportedException("不支持GetMatchObjects方法！");
        }

        #endregion
    }
}
