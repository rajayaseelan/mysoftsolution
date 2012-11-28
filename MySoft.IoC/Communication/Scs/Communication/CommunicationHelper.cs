using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// CommunicationHelper
    /// </summary>
    internal class CommunicationHelper
    {
        /// <summary>
        /// Max communication count.
        /// </summary>
        private const int MaxCommunicationCount = 10000;

        private static readonly TcpSocketAsyncEventArgsPool pool;

        /// <summary>
        /// 实例化CommunicationHelper
        /// </summary>
        static CommunicationHelper()
        {
            pool = new TcpSocketAsyncEventArgsPool(MaxCommunicationCount);

            for (int i = 0; i < MaxCommunicationCount; i++)
            {
                var e = new TcpSocketAsyncEventArgs();
                pool.Push(e);
            }
        }

        /// <summary>
        /// Pool size.
        /// </summary>
        internal static int Size
        {
            get { return MaxCommunicationCount; }
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
        /// <param name="channel"></param>
        /// <returns></returns>
        internal static TcpSocketAsyncEventArgs Pop(ICommunicationCompleted channel)
        {
            lock (pool)
            {
                var item = pool.Pop();
                item.Channel = channel;
                return item;
            }
        }

        /// <summary>
        /// Push SocketAsyncEventArgs.
        /// </summary>
        /// <returns></returns>
        internal static void Push(TcpSocketAsyncEventArgs item)
        {
            if (item == null) return;

            lock (pool)
            {
                item.Channel = null;
                pool.Push(item);
            }
        }
    }
}