using System;
using System.Collections;
using System.Collections.Generic;
using MySoft.IoC.Messages;

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
#if DEBUG
                        Console.WriteLine("Queue Count => {0}\tQueue Key : {1}", queue.Count, queueKey);
#endif
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
