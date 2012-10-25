using System;
using MySoft.IoC.Messages;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Caller item.
    /// </summary>
    internal abstract class CallerItem
    {
        /// <summary>
        /// 调用的Key
        /// </summary>
        public string CallKey { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContext Context { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public RequestMessage Request { get; set; }
    }

    /// <summary>
    /// Thread item
    /// </summary>
    internal class ThreadItem : CallerItem
    {
        public Thread Thread { get; set; }
    }

    /// <summary>
    /// Worker item
    /// </summary>
    internal class WorkerItem : CallerItem
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 间隔时间
        /// </summary>
        public int SlidingTime { get; set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; set; }

        public WorkerItem()
        {
            this.UpdateTime = DateTime.Now;
            this.IsRunning = false;
        }
    }
}
