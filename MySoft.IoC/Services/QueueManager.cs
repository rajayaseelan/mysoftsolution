using System;
using System.Collections.Generic;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 队列管理
    /// </summary>
    internal class QueueManager : IDisposable
    {
        private Queue<ChannelResult> queues;

        /// <summary>
        /// 实例化QueueManager
        /// </summary>
        public QueueManager()
        {
            this.queues = new Queue<ChannelResult>();
        }

        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="result"></param>
        public void Add(ChannelResult result)
        {
            lock (queues)
            {
                queues.Enqueue(result);
            }
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="item"></param>
        public void Set(ResponseItem item)
        {
            if (item == null) return;

            lock (queues)
            {
                while (queues.Count > 0)
                {
                    var result = queues.Dequeue();
                    result.Set(item);
                }
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.queues.Clear();
            this.queues = null;
        }

        #endregion
    }
}