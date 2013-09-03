using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// CommunicationHelper
    /// </summary>
    internal static class CommunicationHelper
    {
        /// <summary>
        /// Max communication count.
        /// </summary>
        private const int MaxCommunicationCount = 10000;

        /// <summary>
        /// Size of the buffer that is used to send bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 4 * 1024; //4KB

        private static SocketAsyncEventArgsPool pool;
        private static BufferManager bufferManager;

        /// <summary>
        /// 实例化CommunicationHelper
        /// </summary>
        static CommunicationHelper()
        {
            pool = new SocketAsyncEventArgsPool(MaxCommunicationCount);
            bufferManager = new BufferManager(MaxCommunicationCount * ReceiveBufferSize, ReceiveBufferSize);
            bufferManager.InitBuffer();

            for (int i = 0; i < MaxCommunicationCount; i++)
            {
                var sae = new TcpSocketAsyncEventArgs();
                bufferManager.SetBuffer(sae);

                pool.Push(sae);
            }
        }

        /// <summary>
        /// Pool count.
        /// </summary>
        internal static int Count
        {
            get
            {
                lock (pool)
                {
                    return pool.Count;
                }
            }
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
                if (item == null) return null;

                if (item.Buffer == null)
                {
                    bufferManager.SetBuffer(item);
                }

                var tcpitem = item as TcpSocketAsyncEventArgs;
                if (tcpitem == null) return null;
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
                if (item.Buffer != null)
                {
                    item.AcceptSocket = null;
                    item.RemoteEndPoint = null;
                    item.UserToken = null;

                    bufferManager.FreeBuffer(item);
                }

                var tcpitem = item as TcpSocketAsyncEventArgs;
                if (tcpitem == null || tcpitem.Channel == null) return;
                tcpitem.Channel = null;

                pool.Push(item);
            }
        }
    }
}