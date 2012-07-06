using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MySoft.IoC.Communication
{
    /// <summary>
    /// Tcp channel
    /// </summary>
    internal interface ITcpChannel
    {
        /// <summary>
        /// Complete receive
        /// </summary>
        /// <param name="e"></param>
        void OnReceiveComplete(SocketAsyncEventArgs e);

        /// <summary>
        /// Complete disconnection
        /// </summary>
        /// <param name="e"></param>
        void OnDisconnectComplete(SocketAsyncEventArgs e);
    }
}
