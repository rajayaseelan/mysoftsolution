using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// Tcp socket async event args.
    /// </summary>
    internal class TcpSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// 回调通道
        /// </summary>
        public ICommunicationProtocol Channel { get; set; }

        /// <summary>
        /// 是否加入队列中
        /// </summary>
        public bool IsPushed { get; set; }

        /// <summary>
        /// 完成事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                //通过通道调用完成方法
                Channel.IOCompleted(e);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
