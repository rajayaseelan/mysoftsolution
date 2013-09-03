using System;
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
        /// 完成事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            if (Channel == null) return;

            try
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        Channel.Received(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send.");
                }
            }
            catch (Exception ex)
            {
                CommunicationHelper.Push(e);
            }
        }
    }
}