using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MySoft.IoC.HttpServer
{
    /// <summary>
    /// CallInfo集合
    /// </summary>
    public sealed class HttpCallerInfoCollection
    {
        private Hashtable hashtable = new Hashtable();

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            lock (hashtable.SyncRoot)
            {
                key = key.ToLower();
                return hashtable.ContainsKey(key);
            }
        }

        /// <summary>
        /// 总数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (hashtable.SyncRoot)
                {
                    return hashtable.Count;
                }
            }
        }

        /// <summary>
        /// 清除集合
        /// </summary>
        public void Clear()
        {
            lock (hashtable.SyncRoot)
            {
                hashtable.Clear();
            }
        }

        /// <summary>
        /// 返回集合
        /// </summary>
        /// <returns></returns>
        public IList<HttpCallerInfo> ToValueList()
        {
            lock (hashtable.SyncRoot)
            {
                return hashtable.Values.Cast<HttpCallerInfo>().ToList();
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
                lock (hashtable.SyncRoot)
                {
                    key = key.ToLower();
                    return hashtable[key] as HttpCallerInfo;
                }
            }
            set
            {
                lock (hashtable.SyncRoot)
                {
                    key = key.ToLower();
                    hashtable[key] = value;
                }
            }
        }
    }
}
