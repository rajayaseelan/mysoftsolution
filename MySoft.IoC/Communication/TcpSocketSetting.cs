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
        public static readonly int AcceptThreads = 5; //接收5个线程

        /// <summary>
        /// 缓冲大小
        /// </summary>
        public static readonly int BufferSize = 8 * 1024; //8kb

        /// <summary>
        /// 最大等待连接数
        /// </summary>
        public static readonly int Backlog = 1000; //1000 连接
    }
}
