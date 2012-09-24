using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// CallInfo集合
    /// </summary>
    public sealed class HttpCallerInfoCollection
    {
        private Hashtable callers = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            key = key.ToLower();
            return callers.ContainsKey(key);
        }

        /// <summary>
        /// 清除集合
        /// </summary>
        public void Clear()
        {
            callers.Clear();
        }

        /// <summary>
        /// 返回集合
        /// </summary>
        /// <returns></returns>
        public IList<HttpCallerInfo> ToValueList()
        {
            return callers.Values.Cast<HttpCallerInfo>().ToList();
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
                return callers[key] as HttpCallerInfo;
            }
            set
            {
                key = key.ToLower();
                callers[key] = value;
            }
        }
    }
}
