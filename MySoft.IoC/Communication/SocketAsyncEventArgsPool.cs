using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Tcp通讯异步套接字池
    /// </summary>
    public class SocketAsyncEventArgsPool
    {
        /// <summary>
        /// SocketAsyncEventArgs栈
        /// </summary>
        private Stack<SocketAsyncEventArgs> pool;

        /// <summary>
        /// 初始化SocketAsyncEventArgs池
        /// </summary>
        /// <param name="capacity">最大可能使用的SocketAsyncEventArgs对象.</param>
        internal SocketAsyncEventArgsPool(Int32 capacity)
        {
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// 返回SocketAsyncEventArgs池中的 数量
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
        /// 弹出一个SocketAsyncEventArgs
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        internal SocketAsyncEventArgs Pop()
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
        /// 添加一个 SocketAsyncEventArgs
        /// </summary>
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }

            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}
