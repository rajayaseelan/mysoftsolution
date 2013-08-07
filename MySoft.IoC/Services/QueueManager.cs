using MySoft.IoC.Messages;
using System;
using System.Collections.Generic;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列管理
    /// </summary>
    internal class QueueManager
    {
        private Queue<WaitResult> queues;

        /// <summary>
        /// 实例化QueueManager
        /// </summary>
        public QueueManager()
        {
            this.queues = new Queue<WaitResult>();
        }

        /// <summary>
        /// 对象数
        /// </summary>
        public int Count
        {
            get
            {
                lock (this.queues)
                {
                    return queues.Count;
                }
            }
        }

        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="result"></param>
        public void Add(WaitResult result)
        {
            lock (this.queues)
            {
                queues.Enqueue(result);
            }
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        public void Set(ResponseMessage resMsg)
        {
            if (resMsg == null) return;

            lock (this.queues)
            {
                try
                {
                    while (queues.Count > 0)
                    {
                        var result = queues.Dequeue();
                        result.Set(resMsg);
                    }
                }
                catch (Exception ex)
                {
                    queues.Clear();
                }
            }
        }
    }
}