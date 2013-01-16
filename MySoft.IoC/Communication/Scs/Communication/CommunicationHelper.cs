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
        private const int MaxCommunicationCount = 2000;

        private static readonly SocketAsyncEventArgsPool pool;

        /// <summary>
        /// 实例化CommunicationHelper
        /// </summary>
        static CommunicationHelper()
        {
            pool = new SocketAsyncEventArgsPool(MaxCommunicationCount);

            for (int i = 0; i < MaxCommunicationCount; i++)
            {
                pool.Push(new SocketAsyncEventArgs());
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
        internal static SocketAsyncEventArgs Pop()
        {
            lock (pool)
            {
                return pool.Pop();
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
                pool.Push(item);
            }
        }
    }
}