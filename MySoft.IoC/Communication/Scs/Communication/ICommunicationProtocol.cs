using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// Communication protocol.
    /// </summary>
    internal interface ICommunicationProtocol
    {
        /// <summary>
        /// On Receive completed.
        /// </summary>
        /// <param name="e"></param>
        void Received(SocketAsyncEventArgs e);
    }
}
