using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.EndPoints;
using MySoft.IoC.Communication.Scs.Communication.Messengers;
using System;

namespace MySoft.IoC.Communication.Scs.Server
{
    /// <summary>
    /// Represents a client from a perspective of a server.
    /// </summary>
    public interface IScsServerClient : IMessenger
    {
        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Unique identifier for this client in server.
        /// </summary>
        long ClientId { get; }

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        ScsEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        CommunicationStates CommunicationState { get; }

        /// <summary>
        /// User token info.
        /// </summary>
        object UserToken { get; set; }

        /// <summary>
        /// Disconnects from server.
        /// </summary>
        void Disconnect();
    }
}
