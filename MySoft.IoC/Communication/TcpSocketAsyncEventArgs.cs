using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Tcp socket event args
    /// </summary>
    internal class TcpSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// Tcp channel
        /// </summary>
        public ITcpChannel Channel { get; set; }

        /// <summary>
        /// Socket pool
        /// </summary>
        internal TcpSocketAsyncEventArgsPool Pool { get; set; }

        /// <summary>
        /// Has queuing
        /// </summary>
        internal bool HasQueuing { get; set; }

        /// <summary>
        /// On complete
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            if (Channel == null) return;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    {
                        Channel.OnSendComplete(e);
                        break;
                    }
                case SocketAsyncOperation.Receive:
                    {
                        Channel.OnReceiveComplete(e);
                        break;
                    }
                case SocketAsyncOperation.Disconnect:
                    {
                        Channel.OnDisconnectComplete(e);
                        break;
                    }
            }
        }
    }
}
