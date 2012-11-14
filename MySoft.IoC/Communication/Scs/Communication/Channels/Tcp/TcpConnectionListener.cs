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
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    OnAcceptCompleted(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            StartSocket();

            //Start accept socket.
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
            IPAddress bindAddress = IPAddress.Any;
            AddressFamily addressFamily = AddressFamily.InterNetwork;

            if (!string.IsNullOrEmpty(_endPoint.IpAddress))
                bindAddress = IPAddress.Parse(_endPoint.IpAddress);

            // Listener socket.
            _listenerSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenerSocket.Bind(new IPEndPoint(bindAddress, _endPoint.TcpPort));
            _listenerSocket.Listen(64);
        }

        /// <summary>
        /// Stops listening socket.
        /// </summary>
        private void StopSocket()
        {
            try
            {
                _listenerSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex) { }
            finally
            {
                try
                {
                    _listenerSocket.Close();
                }
                catch
                {
                }

                _listenerSocket = null;
            }
        }

        /// <summary>
        /// Start accept socket.
        /// </summary>
        private void StartAcceptSocket()
        {
            var e = CreateEventArgs();

            try
            {
                if (!_listenerSocket.AcceptAsync(e))
                {
                    OnAcceptCompleted(e);
                }
            }
            catch (Exception ex)
            {
                DisposeEventArgs(e);

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
                    ThreadPool.QueueUserWorkItem(AcceptCompleted, e);
                }
                else
                {
                    DisposeEventArgs(e);
                }
            }
            catch (Exception ex) { }
            finally
            {
                StartAcceptSocket();
            }
        }

        /// <summary>
        /// Communication channel connected.
        /// </summary>
        /// <param name="state"></param>
        private void AcceptCompleted(object state)
        {
            SocketAsyncEventArgs e = state as SocketAsyncEventArgs;

            try
            {
                if (e.AcceptSocket.Connected)
                {
                    var channel = new TcpCommunicationChannel(e.AcceptSocket);
                    OnCommunicationChannelConnected(channel);
                }
            }
            catch (Exception ex) { }
            finally
            {
                DisposeEventArgs(e);
            }
        }

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateEventArgs()
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

            return e;
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void DisposeEventArgs(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            try
            {
                e.SetBuffer(null, 0, 0);
                e.AcceptSocket = null;
                e.UserToken = null;

                e.Completed -= new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                e.Dispose();
            }
            catch (Exception ex) { }
            finally
            {
                e = null;
            }
        }
    }
}