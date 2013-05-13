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

        private static readonly SocketAsyncEventArgsPool pool;

        /// <summary>
        /// 实例化CommunicationHelper
        /// </summary>
        static CommunicationHelper()
        {
            pool = new SocketAsyncEventArgsPool(MaxCommunicationCount);

            for (int i = 0; i < MaxCommunicationCount; i++)
            {
                pool.Push(new TcpSocketAsyncEventArgs());
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
        /// <param name="channel"></param>
        /// <returns></returns>
        internal static SocketAsyncEventArgs Pop(ICommunicationProtocol channel)
        {
            lock (pool)
            {
                var item = pool.Pop();

                var tcpitem = item as TcpSocketAsyncEventArgs;
                if (tcpitem == null) return null;

                tcpitem.IsPushing = false;
                tcpitem.Channel = channel;

                return item;
            }
        }

        /// <summary>
        /// Push SocketAsyncEventArgs.
        /// </summary>
        /// <returns></returns>
        internal static void Push(SocketAsyncEventArgs item)
        {
            if (item == null) return;

            lock (pool)
            {
                var tcpitem = item as TcpSocketAsyncEventArgs;
                if (tcpitem == null) return;

                if (tcpitem.IsPushing) return;

                tcpitem.IsPushing = true;
                tcpitem.Channel = null;

                pool.Push(item);
            }
        }
    }
}