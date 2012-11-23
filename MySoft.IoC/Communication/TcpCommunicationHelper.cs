using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// TcpCommunicationHelper
    /// </summary>
    internal class TcpCommunicationHelper
    {
        /// <summary>
        /// Max communication count.
        /// </summary>
        private const int MaxCommunicationCount = 10000;

        private static readonly SocketAsyncEventArgsPool pool;

        /// <summary>
        /// 实例化TcpCommunicationHelper
        /// </summary>
        static TcpCommunicationHelper()
        {
            pool = new SocketAsyncEventArgsPool(MaxCommunicationCount);
        }

        /// <summary>
        /// Pop SocketAsyncEventArgs.
        /// </summary>
        /// <returns></returns>
        internal static SocketAsyncEventArgs Pop()
        {
            lock (pool)
            {
                var item = pool.Pop();
                if (item == null)
                {
                    item = new SocketAsyncEventArgs();
                }

                return item;
            }
        }

        /// <summary>
        /// Push SocketAsyncEventArgs.
        /// </summary>
        /// <returns></returns>
        internal static void Push(SocketAsyncEventArgs item)
        {
            lock (pool)
            {
                pool.Push(item);
            }
        }
    }
}