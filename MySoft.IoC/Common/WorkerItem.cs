using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// Worker item
    /// </summary>
    internal class WorkerItem
    {
        /// <summary>
        /// 调用的Key
        /// </summary>
        public string CallKey { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContext Context { get; set; }

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
