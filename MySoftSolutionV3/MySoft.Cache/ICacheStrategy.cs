using System;
using System.Collections.Generic;

namespace MySoft.Cache
{
    /// <summary>
    /// MemoryCache策略接口
    /// </summary>
    public interface IMemoryCacheStrategy : ICacheStrategy
    {
        #region 不指定过期时间

        /// <summary>
        /// 添加指定ID的对象(关联指定文件组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="files"></param>
        void AddObjectWithFileChange(string objId, object o, string[] files);

        /// <summary>
        /// 添加指定ID的对象(关联指定键值组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="dependKey"></param>
        void AddObjectWithDepend(string objId, object o, string[] dependKey);

        #endregion

        #region 指定过期时间

        /// <summary>
        /// 添加指定ID的对象(关联指定文件组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="files"></param>
        void AddObjectWithFileChange(string objId, object o, TimeSpan expires, string[] files);

        /// <summary>
        /// 添加指定ID的对象(关联指定键值组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="dependKey"></param>
        void AddObjectWithDepend(string objId, object o, TimeSpan expires, string[] dependKey);

        #endregion
    }

    /// <summary>
    /// SharedCache策略接口
    /// </summary>
    public interface ISharedCacheStrategy : ICacheStrategy
    {
        /// <summary>
        /// 设置本地缓存超时时间
        /// </summary>
        /// <param name="timeout"></param>
        void SetLocalCacheTimeout(int timeout);
    }

    /// <summary>
    /// 公共缓存策略接口
    /// </summary>
    public interface ICacheStrategy
    {
        /// <summary>
        /// 到期时间
        /// </summary>
        int Timeout { set; get; }

        #region 不指定过期时间

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        void AddObject(string objId, object o);

        #endregion

        #region 指定过期时间

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="expires"></param>
        void AddObject(string objId, object o, TimeSpan expires);

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="datetime"></param>
        void AddObject(string objId, object o, DateTime datetime);

        #endregion

        /// <summary>
        /// 移除指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        void RemoveObject(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        object GetObject(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        T GetObject<T>(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        object GetMatchObject(string regularExpression);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        T GetMatchObject<T>(string regularExpression);

        #region 多对象操作

        /// <summary>
        /// 移除所有缓存对象
        /// </summary>
        void RemoveAllObjects();

        /// <summary>
        /// 获取所有的CacheKey值
        /// </summary>
        /// <returns></returns>
        IList<string> GetAllKeys();

        /// <summary>
        /// 获取缓存数
        /// </summary>
        /// <returns></returns>
        int GetCacheCount();

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetAllObjects();

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDictionary<string, T> GetAllObjects<T>();

        /// <summary>
        /// 通过正则获取对应的Key列表
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        IList<string> GetKeys(string regularExpression);

        /// <summary>
        /// 添加多个对象
        /// </summary>
        /// <param name="data"></param>
        void AddObjects<T>(IDictionary<string, T> data);

        /// <summary>
        /// 添加多个对象
        /// </summary>
        /// <param name="data"></param>
        void AddObjects(IDictionary<string, object> data);

        /// <summary>
        /// 正则表达式方式移除对象
        /// </summary>
        /// <param name="regularExpression">匹配KEY正则表示式</param>
        void RemoveMatchObjects(string regularExpression);

        /// <summary>
        /// 移除指定Key的对象
        /// </summary>
        /// <param name="objIds"></param>
        void RemoveObjects(IList<string> objIds);

        /// <summary>
        /// 返回指定Key的对象
        /// </summary>
        /// <param name="objIds"></param>
        /// <returns></returns>
        IDictionary<string, object> GetObjects(IList<string> objIds);

        /// <summary>
        /// 返回指定Key的对象
        /// </summary>
        /// <param name="objIds"></param>
        /// <returns></returns>
        IDictionary<string, T> GetObjects<T>(IList<string> objIds);

        /// <summary>
        /// 返回指定正则表达式的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        IDictionary<string, object> GetMatchObjects(string regularExpression);

        /// <summary>
        /// 返回指定正则表达的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        IDictionary<string, T> GetMatchObjects<T>(string regularExpression);

        #endregion
    }

}
