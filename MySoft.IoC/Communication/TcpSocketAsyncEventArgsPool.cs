using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Tcp通讯异步套接字池
    /// </summary>
    public class TcpSocketAsyncEventArgsPool
    {
        /// <summary>
        /// TcpSocketAsyncEventArgs栈
        /// </summary>
        private Stack<TcpSocketAsyncEventArgs> pool;

        /// <summary>
        /// 初始化TcpSocketAsyncEventArgs池
        /// </summary>
        /// <param name="capacity">最大可能使用的TcpSocketAsyncEventArgs对象.</param>
        internal TcpSocketAsyncEventArgsPool(Int32 capacity)
        {
            this.pool = new Stack<TcpSocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// 返回TcpSocketAsyncEventArgs池中的 数量
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
        /// 弹出一个TcpSocketAsyncEventArgs
        /// </summary>
        /// <returns>TcpSocketAsyncEventArgs removed from the pool.</returns>
        internal TcpSocketAsyncEventArgs Pop()
        {
            lock (this.pool)
            {
                if (this.Count == 0)
                    return null;
                else
                    return this.pool.Pop();
            }
        }

        /// <summary>
        /// 添加一个 TcpSocketAsyncEventArgs
        /// </summary>
        /// <param name="item">TcpSocketAsyncEventArgs instance to add to the pool.</param>
        internal void Push(TcpSocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a TcpSocketAsyncEventArgsPool cannot be null");
            }

            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}
