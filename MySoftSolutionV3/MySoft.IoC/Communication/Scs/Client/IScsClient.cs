using MySoft.Communication.Scs.Communication;
using MySoft.Communication.Scs.Communication.Messengers;

namespace MySoft.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IScsClient : IMessenger, IConnectableClient
    {
        //Does not define any additional member
        /// <summary>
        /// 是否自动断开
        /// </summary>
        bool AutoDisconnect { get; set; }
    }
}
