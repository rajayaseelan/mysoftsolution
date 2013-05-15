using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// CommunicationHelper
    /// </summary>
    internal class CommunicationHelper
    {
        private volatile SocketAsyncEventArgsPool pool;

        /// <summary>
        /// 实例化CommunicationHelper
        /// </summary>
        /// <param name="maxCommunicationCount"></param>
        public CommunicationHelper(int maxCommunicationCount)
        {
            this.pool = new SocketAsyncEventArgsPool(maxCommunicationCount);

            for (int i = 0; i < maxCommunicationCount; i++)
            {
                pool.Push(new TcpSocketAsyncEventArgs());
            }
        }

        /// <summary>
        /// Pool count.
        /// </summary>
        internal int Count
        {
            get { return pool.Count; }
        }

        /// <summary>
        /// Pop SocketAsyncEventArgs.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        internal SocketAsyncEventArgs Pop(ICommunicationProtocol channel)
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
        internal void Push(SocketAsyncEventArgs item)
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