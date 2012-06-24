using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;
using System;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to listen and accept incoming TCP
    /// connection requests on a TCP port.
    /// </summary>
    internal class TcpConnectionListener : ConnectionListenerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        /// <summary>
        /// Server socket to listen incoming connection requests.
        /// </summary>
        private Socket _listenerSocket;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// Creates a new TcpConnectionListener for given endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        public TcpConnectionListener(ScsTcpEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            StartSocket();
            _running = true;

            //启动多个线程进行接收
            for (int i = 0; i < SocketSetting.AcceptThreads; i++)
            {
                BeginAsyncAccept();
            }
        }

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        public override void Stop()
        {
            _running = false;
            StopSocket();
        }

        /// <summary>
        /// Starts listening socket.
        /// </summary>
        private void StartSocket()
        {
            var endPoint = GetIPEndPoint(_endPoint.IpAddress, _endPoint.TcpPort);

            _listenerSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(endPoint);
            _listenerSocket.Listen(SocketSetting.Backlog * SocketSetting.AcceptThreads);
        }

        /// <summary>
        /// Starts listening socket.
        /// </summary>
        private void BeginAsyncAccept()
        {
            if (!_running) return;

            var e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            if (!_listenerSocket.AcceptAsync(e))
            {
                ThreadPool.QueueUserWorkItem(AsyncAcceptComplete, e);
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                ThreadPool.QueueUserWorkItem(AsyncAcceptComplete, e);
            }
        }

        /// <summary>
        /// Entrance point of the thread.
        /// This method is used by the thread to listen incoming requests.
        /// </summary>
        void AsyncAcceptComplete(object state)
        {
            try
            {
                SocketAsyncEventArgs e = state as SocketAsyncEventArgs;

                if (e.SocketError == SocketError.Success)
                {
                    var clientSocket = e.AcceptSocket;

                    OnCommunicationChannelConnected(new TcpCommunicationChannel(clientSocket));

                    e.AcceptSocket = null;
                }
            }
            catch (Exception ex)
            {

            }

            //重新进行接收
            BeginAsyncAccept();
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
            catch
            {

            }
        }

        /// <summary>
        /// GetIPEndPoint
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private IPEndPoint GetIPEndPoint(string host, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            if (!string.IsNullOrEmpty(host))
            {
                if (!host.Equals("any", StringComparison.CurrentCultureIgnoreCase))
                {
                    IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress s in p.AddressList)
                    {
                        if (s.AddressFamily == AddressFamily.InterNetwork)
                        {
                            endPoint = new IPEndPoint(s, port);
                            break;
                        }
                    }
                }
            }

            return endPoint;
        }
    }
}
