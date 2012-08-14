using System.Net;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to simplify TCP socket operations.
    /// </summary>
    internal static class TcpHelper
    {
        /// <summary>
        /// This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not connect within specified timeoutMs</exception>
        public static TcpClient ConnectToServer(EndPoint endPoint, int timeoutMs)
        {
            var tcpClient = new TcpClient(AddressFamily.InterNetwork);
            try
            {
                tcpClient.Connect(endPoint as IPEndPoint);
                return tcpClient;
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode != 10035)
                {
                    tcpClient.Close();
                    throw;
                }

                if (!tcpClient.Client.Poll(timeoutMs * 1000, SelectMode.SelectWrite))
                {
                    tcpClient.Close();
                    throw new TimeoutException("The host failed to connect. Timeout occured.");
                }

                return tcpClient;
            }
        }
    }
}