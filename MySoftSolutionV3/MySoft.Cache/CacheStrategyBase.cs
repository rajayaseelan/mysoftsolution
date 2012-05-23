using System;
using System.Collections.Generic;

namespace MySoft.Cache
{
    /// <summary>
    /// 缓存基类
    /// </summary>
    public abstract class CacheStrategyBase : ICacheStrategy
    {
        /// <summary>
        /// 分区名称
        /// </summary>
        protected string regionName;
        /// <summary>
        /// 前缀
        /// </summary>
        protected string prefix;

        /// <summary>
        /// 实例化CacheStrategyBase
        /// </summary>
        /// <param name="regionName"></param>
        public CacheStrategyBase(string regionName)
        {
            this.regionName = regionName;

            if (string.IsNullOrEmpty(regionName))
                this.prefix = "{DEFAULT}|";
            else
                this.prefix = "{" + regionName.ToUpper() + "}|";
        }

        // 默认缓存存活期为1440分钟(24小时)
        private int _timeOut = (int)TimeSpan.FromDays(1).TotalSeconds;

        /// <summary>
        /// 设置到期相对时间[单位：秒] 
        /// </summary>
        public int Timeout
        {
            set { _timeOut = value; }
            get { return _timeOut; }
        }

        /// <summary>
        /// 获取输入的Key
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        internal protected string GetInputKey(string objId)
        {
            if (string.IsNullOrEmpty(objId)) return objId;
            if (objId.StartsWith(prefix)) return objId;

            return string.Format("{0}{1}", prefix, objId);
        }

        /// <summary>
        /// 获取输出的Key
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        internal protected string GetOutputKey(string objId)
        {
            if (string.IsNullOrEmpty(objId)) return objId;
            if (!objId.StartsWith(prefix)) return objId;

            return objId.Substring(prefix.Length);
        }

        #region ICacheStrategy 成员

        /// <summary>
        /// 设置区域名称，只能应用于区域名称为空时
        /// </summary>
        /// <param name="regionName"></param>
        public void SetRegionName(string regionName)
        {
            //针对区域名称为空
            if (string.IsNullOrEmpty(this.regionName))
            {
                this.regionName = regionName;

                if (string.IsNullOrEmpty(regionName))
                    this.prefix = "{DEFAULT}|";
                else
                    this.prefix = "{" + regionName.ToUpper() + "}|";
            }
        }

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="datetime"></param>
        public abstract void SetExpired(string objId, DateTime datetime);

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        public abstract void AddObject(string objId, object o);

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="expires"></param>
        public abstract void AddObject(string objId, object o, TimeSpan expires);

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="datetime"></param>
        public abstract void AddObject(string objId, object o, DateTime datetime);

        /// <summary>
        /// 移除指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        public abstract void RemoveObject(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public abstract object GetObject(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public abstract T GetObject<T>(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public abstract object GetMatchObject(string regularExpression);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public abstract T GetMatchObject<T>(string regularExpression);

        /// <summary>
        /// 移除所有缓存对象
        /// </summary>
        public abstract void RemoveAllObjects();

        /// <summary>
        /// 获取所有Key值
        /// </summary>
        /// <returns></returns>
        public abstract IList<string> GetAllKeys();

        /// <summary>
        /// 获取缓存数
        /// </summary>
        /// <returns></returns>
        public abstract int GetCacheCount();

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, object> GetAllObjects();

        /// <summary>
        /// 获取所有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract IDictionary<string, T> GetAllObjects<T>();

        /// <summary>
        /// 通过正则获取对应的Key列表
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public abstract IList<string> GetKeys(string regularExpression);

        /// <summary>
        /// 添加多个对象
        /// </summary>
        /// <param name="data"></param>
        public abstract void AddObjects(IDictionary<string, object> data);

        /// <summary>
        /// 添加多个对象
        /// </summary>
        /// <param name="data"></param>
        public abstract void AddObjects<T>(IDictionary<string, T> data);

        /// <summary>
        /// 正则表达式方式移除对象
        /// </summary>
        /// <param name="regularExpression">匹配KEY正则表示式</param>
        public abstract void RemoveMatchObjects(string regularExpression);

        /// <summary>
        /// 移除多个对象
        /// </summary>
        /// <param name="objIds"></param>
        public abstract void RemoveObjects(IList<string> objIds);

        /// <summary>
        /// 获取多个对象
        /// </summary>
        /// <param name="objIds"></param>
        /// <returns></returns>
        public abstract IDictionary<string, object> GetObjects(IList<string> objIds);

        /// <summary>
        /// 获取多个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objIds"></param>
        /// <returns></returns>
        public abstract IDictionary<string, T> GetObjects<T>(IList<string> objIds);

        /// <summary>
        /// 返回指定正则表达式的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public abstract IDictionary<string, object> GetMatchObjects(string regularExpression);

        /// <summary>
        /// 返回指定正则表达的对象
        /// </summary>
        /// <param name="regularExpression"></param>
        /// <returns></returns>
        public abstract IDictionary<string, T> GetMatchObjects<T>(string regularExpression);

        #endregion
    }
}
