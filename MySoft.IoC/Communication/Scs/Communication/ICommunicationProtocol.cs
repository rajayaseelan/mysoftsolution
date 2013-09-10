using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// Communication protocol.
    /// </summary>
    internal interface ICommunicationProtocol
    {
        /// <summary>
        /// On send completed.
        /// </summary>
        /// <param name="e"></param>
        void SendCompleted(SocketAsyncEventArgs e);

        /// <summary>
        /// On receive completed.
        /// </summary>
        /// <param name="e"></param>
        void ReceiveCompleted(SocketAsyncEventArgs e);
    }
}
