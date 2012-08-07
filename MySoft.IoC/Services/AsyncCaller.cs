using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用服务
    /// </summary>
    /// <param name="service"></param>
    /// <param name="context"></param>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncCaller(IService service, OperationContext context, RequestMessage reqMsg);

    internal sealed class AsyncCallerPool
    {
        /// <summary>
        /// Pool of AsyncCaller.
        /// </summary>
        Stack<AsyncCaller> pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of AsyncCaller objects the pool can hold.</param>
        internal AsyncCallerPool(Int32 capacity)
        {
            this.pool = new Stack<AsyncCaller>(capacity);
        }

        /// <summary>
        /// Get AsyncCaller instance count.
        /// </summary>
        internal int Count
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
        /// Removes a AsyncCaller instance from the pool.
        /// </summary>
        /// <returns>AsyncCaller removed from the pool.</returns>
        internal AsyncCaller Pop()
        {
            lock (this.pool)
            {
                if (this.pool.Count > 0)
                {
                    var item = this.pool.Pop();

                    return item;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool. 
        /// </summary>
        /// <param name="item">AsyncCaller instance to add to the pool.</param>
        internal void Push(AsyncCaller item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a AsyncCallerPool cannot be null");
            }
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}
