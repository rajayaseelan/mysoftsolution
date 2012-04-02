using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using System.Collections;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// QueueManager
    /// </summary>
    internal class QueueManager
    {
        /// <summary>
        /// QueueManager单例
        /// </summary>
        public static readonly QueueManager Instance = new QueueManager();

        /// <summary>
        /// 集合对象
        /// </summary>
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool Add(QueueResult result)
        {
            //如果不缓存，返回false
            if (!result.IsQueuing) return false;

            lock (hashtable.SyncRoot)
            {
                //队列Key
                string queueKey = result.QueueKey;

                if (!hashtable.ContainsKey(queueKey))
                {
                    hashtable[queueKey] = new Queue<QueueResult>();

                    return false;
                }
                else
                {
                    //加入列表
                    var queue = hashtable[queueKey] as Queue<QueueResult>;
                    lock (queue)
                    {
                        queue.Enqueue(result);
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// 响应对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="resMsg"></param>
        public void Set(QueueResult result, ResponseMessage resMsg)
        {
            if (!result.IsQueuing) return;

            lock (hashtable.SyncRoot)
            {
                //队列Key
                string queueKey = result.QueueKey;

                if (hashtable.ContainsKey(queueKey))
                {
                    var queue = hashtable[queueKey] as Queue<QueueResult>;
                    hashtable.Remove(queueKey);

                    if (queue.Count > 0)
                    {
                        Console.WriteLine("Queue Count => {0}\tQueue Key : {1}", queue.Count, queueKey);

                        while (queue.Count > 0)
                        {
                            var waitResult = queue.Dequeue();

                            //响应消息
                            waitResult.Set(resMsg);
                        }
                    }
                }
            }
        }
    }
}
