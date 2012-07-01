using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication;

namespace MySoft.IoC.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to simplify TCP socket operations.
    /// </summary>
    internal class TcpHelper
    {
        private EndPoint endPoint;
        private AutoResetEvent waitReset = new System.Threading.AutoResetEvent(false);

        public TcpHelper(EndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        /// <summary>
        /// This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not connect within specified timeoutMs</exception>
        public Socket ConnectToServer(int timeoutMs)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            e.RemoteEndPoint = endPoint;
            e.UserToken = socket;

            try
            {
                if (!socket.ConnectAsync(e))
                {
                    AsyncConnectComplete(e);
                }

                if (!waitReset.WaitOne(timeoutMs))
                {
                    throw new TimeoutException("The host failed to connect. Timeout occured.");
                }

                if (e.SocketError != SocketError.Success)
                {
                    throw new CommunicationException("The host failed to connect.");
                }

                return socket;
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Dispose(e);

                throw ex;
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                AsyncConnectComplete(e);
            }
        }

        void AsyncConnectComplete(SocketAsyncEventArgs e)
        {
            //响应消息
            waitReset.Set();

            try
            {
                Socket socket = e.UserToken as Socket;

                if (e.SocketError != SocketError.Success)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch
            {

            }
            finally
            {
                TcpSocketHelper.Dispose(e);
            }
        }
    }
}
