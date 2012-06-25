using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Socket helper class
    /// </summary>
    internal static class TcpSocketHelper
    {
        /// <summary>
        /// 释放资源，设置buffer为null，userToken为null，同时调用 dispose方法
        /// </summary>
        /// <param name="e"></param>
        public static void Release(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            e.AcceptSocket = null;
            e.RemoteEndPoint = null;
            e.SetBuffer(null, 0, 0);
            e.UserToken = null;
        }

        /// <summary>
        /// 释放资源，同时将e设置为null
        /// </summary>
        /// <param name="e"></param>
        public static void Dispose(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            Release(e);

            e.Dispose();
            e = null;
        }
    }
}
