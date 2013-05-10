using System;
using System.Collections;
using System.Collections.Generic;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列管理
    /// </summary>
    internal class QueueManager
    {
        /// <summary>
        /// 实例化队列
        /// </summary>
        private readonly Queue<ChannelResult> queue = new Queue<ChannelResult>();

        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="result"></param>
        public void Add(ChannelResult result)
        {
            lock (queue)
            {
                queue.Enqueue(result);
            }
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="item"></param>
        public void Set(ResponseItem item)
        {
            lock (queue)
            {
                while (queue.Count > 0)
                {
                    var result = queue.Dequeue();
                    if (result == null) continue;

                    result.Set(item);
                }
            }
        }

        /// <summary>
        /// 队列长度
        /// </summary>
        public int Count
        {
            get
            {
                lock (queue)
                {
                    return queue.Count;
                }
            }
        }

        /// <summary>
        /// 清除队列
        /// </summary>
        public void Clear()
        {
            lock (queue)
            {
                queue.Clear();
            }
        }
    }
}