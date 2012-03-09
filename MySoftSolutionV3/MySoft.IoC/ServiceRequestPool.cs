using System;
using System.Collections.Generic;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务消息池
    /// </summary>
    public sealed class ServiceRequestPool
    {
        /// <summary>
        /// ServiceRequest栈
        /// </summary>
        private Stack<ServiceRequest> pool;

        /// <summary>
        /// 初始化ServiceRequest池
        /// </summary>
        /// <param name="capacity">最大可能使用的ServiceRequest对象.</param>
        internal ServiceRequestPool(Int32 capacity)
        {
            this.pool = new Stack<ServiceRequest>(capacity);
        }

        /// <summary>
        /// 返回ServiceRequest池中的 数量
        /// </summary>
        internal Int32 Count
        {
            get
            {
                lock (this.pool)
                {
                    return this.pool.Count;
                }
            }
        }

        /// <summary>
        /// 弹出一个ServiceRequest
        /// </summary>
        /// <returns>ServiceRequest removed from the pool.</returns>
        internal ServiceRequest Pop()
        {
            lock (this.pool)
            {
                if (this.Count > 0)
                    return this.pool.Pop();
                else
                    return null;
            }
        }

        /// <summary>
        /// 添加一个 ServiceRequest
        /// </summary>
        /// <param name="item">ServiceRequest instance to add to the pool.</param>
        internal void Push(ServiceRequest item)
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
