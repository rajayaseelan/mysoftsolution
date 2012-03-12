using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// WaitResult集合
    /// </summary>
    public class WaitResultCollection
    {
        private IDictionary<Guid, WaitResult> hashtable;

        /// <summary>
        /// 实例化WaitResultCollection
        /// </summary>
        public WaitResultCollection()
        {
            this.hashtable = new Dictionary<Guid, WaitResult>();
        }

        /// <summary>
        /// get or set wait result
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public WaitResult this[Guid key]
        {
            get
            {
                return hashtable[key];
            }
            set
            {
                lock (hashtable)
                {
                    hashtable[key] = value;
                }
            }
        }

        /// <summary>
        /// 判断是否存在对应的key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(Guid key)
        {
            return hashtable.ContainsKey(key);
        }

        /// <summary>
        /// 移除指定key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(Guid key)
        {
            lock (hashtable)
            {
                return hashtable.Remove(key);
            }
        }
    }

    /// <summary>
    /// 返回值对象
    /// </summary>
    [Serializable]
    public sealed class WaitResult
    {
        private AutoResetEvent reset;
        /// <summary>
        /// 信号量对象
        /// </summary>
        public AutoResetEvent Reset
        {
            get { return reset; }
        }

        private ResponseMessage message;
        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// 实例化WaitResult
        /// </summary>
        public WaitResult()
        {
            this.reset = new AutoResetEvent(false);
        }
    }
}
