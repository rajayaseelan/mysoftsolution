using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// SocketSetting类
    /// </summary>
    public static class SocketSetting
    {
        static SocketSetting()
        {
            Set(MaxConnections, BufferSize);
        }

        /// <summary>
        /// 接收线程数
        /// </summary>
        public static int AcceptThreads = 3; //接收3个线程

        /// <summary>
        /// 缓冲大小
        /// </summary>
        public static int BufferSize = 8 * 1024; //8kb

        /// <summary>
        /// 最大连接数
        /// </summary>
        public static int MaxConnections = 10000; //10000个连接

        /// <summary>
        /// 最大等待连接数
        /// </summary>
        public static int Backlog = 1024; //1024 连接

        private static SocketAsyncEventArgsPool _socketPool;
        /// <summary>
        /// Tcp socket池
        /// </summary>
        internal static SocketAsyncEventArgsPool TcpSocketPool
        {
            get { return _socketPool; }
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="maxConnections"></param>
        public static void Set(int maxConnections)
        {
            Set(maxConnections, BufferSize);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="maxConnections"></param>
        /// <param name="bufferSize"></param>
        public static void Set(int maxConnections, int bufferSize)
        {
            Set(maxConnections, bufferSize, Backlog);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="maxConnections"></param>
        /// <param name="bufferSize"></param>
        /// <param name="backlog"></param>
        public static void Set(int maxConnections, int bufferSize, int backlog)
        {
            MaxConnections = maxConnections;
            BufferSize = bufferSize;
            Backlog = backlog;

            //初始化池
            _socketPool = new SocketAsyncEventArgsPool(maxConnections);

            lock (_socketPool)
            {
                for (int i = 0; i < maxConnections; i++)
                {
                    _socketPool.Push(new SocketAsyncEventArgs());
                }
            }
        }
    }
}
