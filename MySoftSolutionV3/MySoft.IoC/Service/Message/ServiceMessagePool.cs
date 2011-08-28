using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Message
{
    /// <summary>
    /// 服务消息池
    /// </summary>
    internal sealed class ServiceMessagePool
    {
        /// <summary>
        /// ServiceRequest栈
        /// </summary>
        private Stack<ServiceMessage> pool;

        /// <summary>
        /// 初始化ServiceRequest池
        /// </summary>
        /// <param name="capacity">最大可能使用的ServiceRequest对象.</param>
        internal ServiceMessagePool(Int32 capacity)
        {
            this.pool = new Stack<ServiceMessage>(capacity);
        }

        /// <summary>
        /// 返回ServiceRequest池中的 数量
        /// </summary>
        internal Int32 Count
        {
            get { return this.pool.Count; }
        }

        /// <summary>
        /// 弹出一个ServiceRequest
        /// </summary>
        /// <returns>ServiceRequest removed from the pool.</returns>
        internal ServiceMessage Pop()
        {
            lock (this.pool)
            {
                return this.pool.Pop();
            }
        }

        /// <summary>
        /// 添加一个 ServiceRequest
        /// </summary>
        /// <param name="item">ServiceRequest instance to add to the pool.</param>
        internal void Push(ServiceMessage item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a ServiceRequestPool cannot be null");
            }
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}
