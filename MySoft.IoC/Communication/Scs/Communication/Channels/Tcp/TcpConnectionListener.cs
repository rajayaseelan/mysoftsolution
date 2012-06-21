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

            for (int i = 0; i < SocketConfig.AcceptThreads; i++)
            {
                Thread _thread = new Thread(BeginAccept);
                _thread.Start();
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
            _listenerSocket.Listen(SocketConfig.Backlog * SocketConfig.AcceptThreads);
        }

        /// <summary>
        /// GetIPEndPoint
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private IPEndPoint GetIPEndPoint(string host, int port)
        {
            IPEndPoint myEnd = new IPEndPoint(IPAddress.Any, port);

            if (!string.IsNullOrEmpty(host))
            {
                if (!host.Equals("any", StringComparison.CurrentCultureIgnoreCase))
                {
                    IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress s in p.AddressList)
                    {
                        if (s.AddressFamily == AddressFamily.InterNetwork)
                        {
                            myEnd = new IPEndPoint(s, port);
                            break;
                        }
                    }
                }
            }

            return myEnd;
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
            finally
            {
                _listenerSocket = null;
            }
        }

        /// <summary>
        /// Entrance point of the thread.
        /// This method is used by the thread to listen incoming requests.
        /// </summary>
        private void BeginAccept()
        {
            try
            {
                var socketAsyncArgs = new SocketAsyncEventArgs();
                socketAsyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                socketAsyncArgs.UserToken = _listenerSocket;

                //接收监听
                if (!_listenerSocket.AcceptAsync(socketAsyncArgs))
                    IO_Completed(this, socketAsyncArgs);
            }
            catch
            {
                //Disconnect, wait for a while and connect again.
                StopSocket();

                Thread.Sleep(1000);

                if (!_running)
                {
                    return;
                }

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
        /// 接收完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptComplete), e);
        }

        /// <summary>
        /// 接收完成
        /// </summary>
        /// <param name="state"></param>
        private void AcceptComplete(object state)
        {
            if (!_running) return;

            SocketAsyncEventArgs e = state as SocketAsyncEventArgs;

            try
            {
                Socket listener = e.UserToken as Socket;

                if (e.SocketError == SocketError.Success)
                {
                    OnCommunicationChannelConnected(new TcpCommunicationChannel(listener));
                }

                BeginAccept();
            }
            catch
            {
                //Disconnect, wait for a while and connect again.
                StopSocket();

                Thread.Sleep(1000);

                if (!_running)
                {
                    return;
                }

                try
                {
                    StartSocket();
                }
                catch
                {

                }
            }
        }
    }
}
