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
        /// <param name="objId"></param>
        public static void Remove(string objId)
        {
            lock (lockObject)
            {
                if (objId == null || objId.Length == 0)
                {
                    return;
                }

                webCache.Remove(objId);
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                foreach (string objId in AllKeys)
                {
                    webCache.Remove(objId);
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
                    IList<string> objIds = new List<string>();

                    while (cacheEnum.MoveNext())
                    {
                        objIds.Add(cacheEnum.Key.ToString());
                    }

                    return objIds;
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
        /// <param name="regionName"></param>
        public LocalCacheStrategy(string regionName) : base(regionName) { }

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
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public override void AddObject(string objId, object o)
        {
            if (objId == null || objId.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                if (Timeout <= 0)
                {
                    webCache.Insert(GetInputKey(objId), o, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
                else
                {
                    webCache.Insert(GetInputKey(objId), o, null, DateTime.Now.AddSeconds(Timeout), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public override void AddObject(string objId, object o, TimeSpan expires)
        {
            AddObject(objId, o, DateTime.Now.Add(expires));
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public override void AddObject(string objId, object o, DateTime datetime)
        {
            if (objId == null || objId.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                webCache.Insert(GetInputKey(objId), o, null, datetime, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并对相关文件建立依赖
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="files">监视的路径文件</param>
        public void AddObjectWithFileChange(string objId, object o, string[] files)
        {
            if (objId == null || objId.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(files, DateTime.Now);

                if (Timeout <= 0)
                {
                    webCache.Insert(GetInputKey(objId), o, dep, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
                else
                {
                    webCache.Insert(GetInputKey(objId), o, dep, System.DateTime.Now.AddSeconds(Timeout), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
            }
        }


        /// <summary>
        /// 加入当前对象到缓存中,并使用依赖键
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="dependKey">依赖关联的键值</param>
        public void AddObjectWithDepend(string objId, object o, string[] dependKey)
        {
            if (objId == null || objId.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(null, dependKey, DateTime.Now);

                if (Timeout <= 0)
                {
                    webCache.Insert(GetInputKey(objId), o, dep, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
                else
                {
                    webCache.Insert(GetInputKey(objId), o, dep, System.DateTime.Now.AddSeconds(Timeout), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
                }
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并对相关文件建立依赖
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="files">监视的路径文件</param>
        public void AddObjectWithFileChange(string objId, object o, TimeSpan expires, string[] files)
        {
            if (objId == null || objId.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(files, DateTime.Now);

                webCache.Insert(GetInputKey(objId), o, dep, System.DateTime.Now.Add(expires), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
            }
        }


        /// <summary>
        /// 加入当前对象到缓存中,并使用依赖键
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="dependKey">依赖关联的键值</param>
        public void AddObjectWithDepend(string objId, object o, TimeSpan expires, string[] dependKey)
        {
            if (objId == null || objId.Length == 0 || o == null)
            {
                return;
            }

            lock (lockObject)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(onRemove);

                CacheDependency dep = new CacheDependency(null, dependKey, DateTime.Now);

                webCache.Insert(GetInputKey(objId), o, dep, System.DateTime.Now.Add(expires), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, callBack);
            }
        }

        /// <summary>
        /// 建立回调委托的一个实例
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="val"></param>
        /// <param name="reason"></param>
        public void onRemove(string objId, object val, CacheItemRemovedReason reason)
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
            //myLogVisitor.WriteLog(this,objId,val,reason);

            //SimpleLog.Instance.WriteLogForDir("Cache", reason.ToString() + "：" + objId);
            //MemoryManager.FlushMemory();
        }

        #region ICacheStrategy 成员

        /// <summary>
        /// 删除缓存对象
        /// </summary>
        /// <param name="objId">对象的关键字</param>
        public override void RemoveObject(string objId)
        {
            if (objId == null || objId.Length == 0)
            {
                return;
            }

            lock (lockObject)
            {
                webCache.Remove(GetInputKey(objId));
            }
        }

        /// <summary>
        /// 返回一个指定的对象
        /// </summary>
        /// <param name="objId">对象的关键字</param>
        /// <returns>对象</returns>
        public override object GetObject(string objId)
        {
            if (objId == null || objId.Length == 0)
            {
                return null;
            }

            lock (lockObject)
            {
                return webCache.Get(GetInputKey(objId));
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public override T GetObject<T>(string objId)
        {
            if (objId == null || objId.Length == 0)
            {
                return default(T);
            }

            lock (lockObject)
            {
                return (T)GetObject(GetInputKey(objId));
            }
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
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
        /// <param name="objId"></param>
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
                List<string> objIds = new List<string>();

                while (cacheEnum.MoveNext())
                {
                    objIds.Add(cacheEnum.Key.ToString());
                }

                objIds.RemoveAll(objId => !objId.StartsWith(prefix));
                return objIds.ConvertAll<string>(objId => GetOutputKey(objId));
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

                IList<string> objIds = new List<string>();
                Regex regex = new Regex(regularExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

                foreach (var objId in GetAllKeys())
                {
                    if (regex.IsMatch(objId)) objIds.Add(objId);
                }

                return objIds;
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
                var objIds = GetKeys(regularExpression);
                RemoveObjects(objIds);
            }
        }

        /// <summary>
        /// 移除多个对象
        /// </summary>
        /// <param name="objIds"></param>
        public override void RemoveObjects(IList<string> objIds)
        {
            lock (lockObject)
            {
                foreach (string objId in objIds)
                {
                    RemoveObject(GetInputKey(objId));
                }
            }
        }

        /// <summary>
        /// 获取多个对象
        /// </summary>
        /// <param name="objIds"></param>
        /// <returns></returns>
        public override IDictionary<string, object> GetObjects(IList<string> objIds)
        {
            lock (lockObject)
            {
                IDictionary<string, object> cacheData = new Dictionary<string, object>();
                foreach (string objId in objIds)
                {
                    var data = GetObject(GetInputKey(objId));
                    if (data != null)
                        cacheData.Add(objId, data);
                    else
                        cacheData.Add(objId, null);
                }

                return cacheData;
            }
        }

        /// <summary>
        /// 获取多个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objIds"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetObjects<T>(IList<string> objIds)
        {
            lock (lockObject)
            {
                IDictionary<string, T> cacheData = new Dictionary<string, T>();
                foreach (string objId in objIds)
                {
                    var data = GetObject<T>(GetInputKey(objId));
                    if (data != null)
                        cacheData.Add(objId, data);
                    else
                        cacheData.Add(objId, default(T));
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
