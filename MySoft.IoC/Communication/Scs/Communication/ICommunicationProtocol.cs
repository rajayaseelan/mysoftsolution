using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication
{
    /// <summary>
    /// Communication protocol.
    /// </summary>
    internal interface ICommunicationProtocol
    {
        /// <summary>
        /// On IO completed.
        /// </summary>
        /// <param name="e"></param>
        void IOCompleted(SocketAsyncEventArgs e);
    }
}
