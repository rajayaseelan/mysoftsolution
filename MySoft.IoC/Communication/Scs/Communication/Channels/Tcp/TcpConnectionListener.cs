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
    internal class TcpConnectionListener : ConnectionListenerBase, ICommunicationCompleted
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
        /// <param name="e"></param>
        void ICommunicationCompleted.IOCompleted(SocketAsyncEventArgs e)
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

            var endPoint = new IPEndPoint(bindAddress, _endPoint.TcpPort);

            // Listener socket.
            _listenerSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenerSocket.Bind(endPoint);
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
            var e = PopSocketEventArgs();

            try
            {
                if (!_listenerSocket.AcceptAsync(e))
                {
                    OnAcceptCompleted(e);
                }
            }
            catch (Exception ex)
            {
                PushSocketEventArgs(e);

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
                if (e.AcceptSocket.Connected)
                {
                    var channel = new TcpCommunicationChannel(e.AcceptSocket);
                    OnCommunicationChannelConnected(channel);
                }
            }
            catch (Exception ex) { }
            finally
            {
                PushSocketEventArgs(e);
            }

            //重新开始接收
            StartAcceptSocket();
        }

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs PopSocketEventArgs()
        {
            return CommunicationHelper.Pop(this);
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void PushSocketEventArgs(SocketAsyncEventArgs e)
        {
            try
            {
                e.SetBuffer(null, 0, 0);
                e.AcceptSocket = null;
                e.UserToken = null;
            }
            catch (Exception ex) { }
            finally
            {
                var tcp = e as TcpSocketAsyncEventArgs;
                CommunicationHelper.Push(tcp);
            }
        }
    }
}