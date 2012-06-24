using MySoft.IoC.Communication.Scs.Communication.Channels;
using MySoft.IoC.Communication.Scs.Communication.Channels.Tcp;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.EndPoints;

namespace MySoft.IoC.Communication.Scs.Server.Tcp
{
    /// <summary>
    /// This class is used to create a TCP server.
    /// </summary>
    internal class ScsTcpServer : ScsServerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        public override ScsEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        /// <summary>
        /// Creates a new ScsTcpServer object.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        public ScsTcpServer(ScsTcpEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        /// <summary>
        /// Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpConnectionListener(_endPoint);
        }
    }
}
