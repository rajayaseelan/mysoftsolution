using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// TcpSocketSetting类
    /// </summary>
    public static class TcpSocketSetting
    {
        /// <summary>
        /// 接收线程数
        /// </summary>
        public const int AcceptThreads = 10; //接收10个线程

        /// <summary>
        /// 最大等待连接数
        /// </summary>
        public const int Backlog = 100; //100 连接

        /// <summary>
        /// 连接池对象
        /// </summary>
        internal static TcpSocketAsyncEventArgsPool SocketPool;

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="maxConnections"></param>
        internal static void Init(int maxConnections)
        {
            if (SocketPool == null)
            {
                if (maxConnections < 100) maxConnections = 100;
                SocketPool = new TcpSocketAsyncEventArgsPool(maxConnections);

                lock (SocketPool)
                {
                    //实例化n个对象
                    for (int i = 0; i < maxConnections; i++)
                    {
                        var e = new TcpSocketAsyncEventArgs();
                        e.Pool = SocketPool;

                        //入队列
                        SocketPool.Push(e);
                    }
                }
            }
        }
    }
}
