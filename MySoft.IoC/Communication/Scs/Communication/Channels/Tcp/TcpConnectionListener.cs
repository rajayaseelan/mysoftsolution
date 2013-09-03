using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            StartAcceptSocket();
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
            // Get endpoint for the listener.
            var bindAddress = IPAddress.Any;

            if (!string.IsNullOrEmpty(_endPoint.IpAddress))
                bindAddress = IPAddress.Parse(_endPoint.IpAddress);

            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                _listenerSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                _listenerSocket = null;
            }
        }

        /// <summary>
        /// Entrance point of the thread.
        /// This method is used by the thread to listen incoming requests.
        /// </summary>
        private void StartAcceptSocket()
        {
            if (!_running) return;

            try
            {
                var e = new SocketAsyncEventArgs();
                e.Completed += IO_Completed;

                if (!_listenerSocket.AcceptAsync(e))
                {
                    IO_Completed(null, e);
                }
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
        /// IO_Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Accept)
                {
                    //Async accept socket.
                    ThreadPool.QueueUserWorkItem(AcceptCompleted, e);
                }
            }

            StartAcceptSocket();
        }

        /// <summary>
        /// Accept completed.
        /// </summary>
        /// <param name="state"></param>
        void AcceptCompleted(object state)
        {
            if (state == null) return;

            var e = state as SocketAsyncEventArgs;

            try
            {
                var clientSocket = e.AcceptSocket;
                if (clientSocket.Connected)
                {
                    OnCommunicationChannelConnected(new TcpCommunicationChannel(clientSocket, true));
                }
            }
            catch (Exception ex) { }
            finally
            {
                e.AcceptSocket = null;
                e.Dispose();
            }
        }
    }
}