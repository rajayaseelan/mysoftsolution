using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// CallInfo集合
    /// </summary>
    public sealed class HttpCallerInfoCollection : IEnumerable<KeyValuePair<string, HttpCallerInfo>>
    {
        private IDictionary<string, HttpCallerInfo> callers;

        /// <summary>
        /// 实例化HttpCallerInfoCollection
        /// </summary>
        public HttpCallerInfoCollection()
        {
            this.callers = new Dictionary<string, HttpCallerInfo>();
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            lock (callers)
            {
                key = key.ToLower();
                return callers.ContainsKey(key);
            }
        }

        /// <summary>
        /// 获取集合中的数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HttpCallerInfo this[string key]
        {
            get
            {
                key = key.ToLower();
                return callers[key];
            }
            set
            {
                key = key.ToLower();
                callers[key] = value;
            }
        }

        #region IEnumerable<KeyValuePair<string,HttpCallerInfo>> 成员

        /// <summary>
        /// 获取集合枚举
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, HttpCallerInfo>> GetEnumerator()
        {
            return callers.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        /// <summary>
        /// 获取集合枚举
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return callers.GetEnumerator();
        }

        #endregion
    }
}
