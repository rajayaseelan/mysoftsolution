using MySoft.IoC.Communication.Scs.Communication.Messengers;

namespace MySoft.IoC.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IScsClient : IMessenger, IConnectableClient
    {
        //Does not define any additional member
        bool KeepAlive { get; set; }
    }
}
