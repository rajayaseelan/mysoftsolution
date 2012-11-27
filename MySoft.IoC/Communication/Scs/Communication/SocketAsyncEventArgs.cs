using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// Tcp通讯异步套接字
    /// </summary>
    internal class TcpSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// 数据处理通道
        /// </summary>
        public ICommunicationCompleted Channel { get; set; }

        /// <summary>
        /// 异步完成回调
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            if (Channel == null) return;

            //调用完成事件
            Channel.IOCompleted(e);
        }
    }
}
