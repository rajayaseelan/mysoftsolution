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
        private const int MaxCommunicationCount = 1000;

        private static readonly SocketAsyncEventArgsPool pool;

        /// <summary>
        /// 实例化TcpCommunicationHelper
        /// </summary>
        static TcpCommunicationHelper()
        {
            pool = new SocketAsyncEventArgsPool(MaxCommunicationCount);

            for (int i = 0; i < MaxCommunicationCount; i++)
            {
                var e = new SocketAsyncEventArgs();
                pool.Push(e);
            }
        }

        /// <summary>
        /// Pool count.
        /// </summary>
        internal static int Count
        {
            get { return pool.Count; }
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