using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Tcp通讯异步套接字
    /// </summary>
    public class TcpSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// 数据处理通道
        /// </summary>
        public ITcpSocketChannel Channel { get; set; }

        /// <summary>
        /// 异步完成回调
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    Channel.OnSendCompleted(e);
                    break;
                case SocketAsyncOperation.Receive:
                    Channel.OnReceiveCompleted(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }
    }
}
