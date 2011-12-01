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
        /// 连接断开时间
        /// </summary>
        int DisconnectTimeout { get; set; }
    }
}
