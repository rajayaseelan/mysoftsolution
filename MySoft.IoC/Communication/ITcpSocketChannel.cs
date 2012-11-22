using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// 通讯通道接口
    /// </summary>
    public interface ITcpSocketChannel
    {
        /// <summary>
        /// 接收完成处理
        /// </summary>
        /// <param name="e"></param>
        void OnReceiveCompleted(SocketAsyncEventArgs e);

        /// <summary>
        /// 发送完成处理
        /// </summary>
        /// <param name="e"></param>
        void OnSendCompleted(SocketAsyncEventArgs e);
    }
}
