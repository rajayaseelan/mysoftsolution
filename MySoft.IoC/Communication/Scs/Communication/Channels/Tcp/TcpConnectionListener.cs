using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to listen and accept incoming TCP
    /// connection requests on a TCP port.
    /// </summary>
    internal class TcpConnectionListener : ConnectionListenerBase
    {
        private const int SOCKET_BACKLOG = 1024;

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 2 * 1024; //2KB

        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        /// <summary>
        /// Server socket to listen incoming connection requests.
        /// </summary>
        private Socket _listenerSocket;

        /// <summary>
        /// Creates a new TcpConnectionListener for given endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        public TcpConnectionListener(ScsTcpEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        /// <summary>
        /// IO回调处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Accept)
                {
                    OnAcceptCompleted(e);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            StartSocket();
            StartAcceptSocket();
        }

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        public override void Stop()
        {
            StopSocket();
        }

        /// <summary>
        /// Starts listening socket.
        /// </summary>
        private void StartSocket()
        {
            // Get endpoint for the listener.
            IPEndPoint endPoint = null;

            if (string.IsNullOrEmpty(_endPoint.IpAddress))
                endPoint = new IPEndPoint(IPAddress.Any, _endPoint.TcpPort);
            else
                endPoint = new IPEndPoint(IPAddress.Parse(_endPoint.IpAddress), _endPoint.TcpPort);

            // Listener socket.
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            _listenerSocket.SendBufferSize = BufferSize;
            _listenerSocket.ReceiveBufferSize = BufferSize;
            _listenerSocket.Bind(endPoint);
            _listenerSocket.Listen(SOCKET_BACKLOG);
        }

        /// <summary>
        /// Stops listening socket.
        /// </summary>
        private void StopSocket()
        {
            try
            {
                _listenerSocket.Shutdown(SocketShutdown.Both);
                _listenerSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                _listenerSocket = null;
            }
        }

        /// <summary>
        /// Start accept socket.
        /// </summary>
        private void StartAcceptSocket()
        {
            try
            {
                var _acceptEventArgs = new SocketAsyncEventArgs();
                _acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

                if (!_listenerSocket.AcceptAsync(_acceptEventArgs))
                {
                    OnAcceptCompleted(_acceptEventArgs);
                }
            }
            catch (Exception ex)
            {
                StopSocket();

                //Thread 1000 ms.
                Thread.Sleep(1000);

                try
                {
                    StartSocket();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Socket accept completed.
        /// </summary>
        /// <param name="e"></param>
        private void OnAcceptCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var clientSocket = e.AcceptSocket;
                    if (clientSocket.Connected)
                    {
                        OnCommunicationChannelConnected(new TcpCommunicationChannel(clientSocket));
                    }

                    e.AcceptSocket = null;
                }
            }
            catch (Exception ex) { }
            finally
            {
                try
                {
                    e.Completed -= new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                    e.SetBuffer(null, 0, 0);
                }
                catch (Exception ex) { }
                finally
                {
                    e.Dispose();
                }

                StartAcceptSocket();
            }
        }
    }
}