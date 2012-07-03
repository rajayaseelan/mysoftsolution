using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.socketasynceventargs.aspx
    /// Represents a collection of reusable SocketAsyncEventArgs objects.  
    /// </summary>
    internal sealed class TcpSocketAsyncEventArgsPool
    {
        /// <summary>
        /// Pool of SocketAsyncEventArgs.
        /// </summary>
        Stack<TcpSocketAsyncEventArgs> pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
        internal TcpSocketAsyncEventArgsPool(Int32 capacity)
        {
            this.pool = new Stack<TcpSocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// Get SocketAsyncEventArgs instance count.
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
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        internal TcpSocketAsyncEventArgs Pop(ITcpChannel channel)
        {
            lock (this.pool)
            {
                if (this.pool.Count > 0)
                {
                    var item = this.pool.Pop();
                    item.Channel = channel;
                    item.HasQueuing = false;

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
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        internal void Push(TcpSocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            lock (this.pool)
            {
                item.Channel = null;
                item.HasQueuing = true;

                this.pool.Push(item);
            }
        }
    }
}
