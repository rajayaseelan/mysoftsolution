using System;
using System.Net;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Communication.EndPoints;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    internal class TcpCommunicationChannel : CommunicationChannelBase
    {
        #region Public properties

        ///<summary>
        /// Gets the endpoint of remote application.
        ///</summary>
        public override ScsEndPoint RemoteEndPoint
        {
            get
            {
                return _remoteEndPoint;
            }
        }
        private readonly ScsTcpEndPoint _remoteEndPoint;

        #endregion

        #region Private fields

        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private readonly Socket _clientSocket;

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 1024; //1KB

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private volatile byte[] _buffer;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="clientSocket">A connected Socket object that is
        /// used to communicate over network</param>
        public TcpCommunicationChannel(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _clientSocket.NoDelay = true;

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);

            _buffer = new byte[ReceiveBufferSize];
        }

        #endregion

        #region Public methods

        private void Disconnect(SocketAsyncEventArgs e)
        {
            Dispose(e);

            Disconnect();
        }

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
            }

            _buffer = null;
            WireProtocol.Reset();

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

            try
            {
                e.SetBuffer(_buffer, 0, _buffer.Length);

                if (!_clientSocket.ReceiveAsync(e))
                {
                    OnReceiveCompleted(e);
                }
            }
            catch (ObjectDisposedException)
            {
                Dispose(e);
            }
            catch (SocketException ex)
            {
                Disconnect(e);

                throw ex;
            }
            catch (Exception ex)
            {
                Dispose(e);

                OnMessageError(ex);

                throw ex;
            }
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            //Create a byte array from message according to current protocol
            var messageBytes = WireProtocol.GetBytes(message);

            var e = new SocketAsyncEventArgs();
            e.UserToken = message;
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

            try
            {
                e.SetBuffer(messageBytes, 0, messageBytes.Length);

                //Send all bytes to the remote application
                if (!_clientSocket.SendAsync(e))
                {
                    OnSendCompleted(e);
                }
            }
            catch (ObjectDisposedException)
            {
                Dispose(e);
            }
            catch (SocketException ex)
            {
                Disconnect(e);

                throw ex;
            }
            catch (Exception ex)
            {
                Dispose(e);

                OnMessageError(ex);

                throw ex;
            }
        }

        #endregion

        #region Private methods

        private void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                OnSendCompleted(e);
            }
            else if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                OnReceiveCompleted(e);
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void OnSendCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                //Record last sent time
                LastSentMessageTime = DateTime.Now;

                //Sent success
                OnMessageSent(e.UserToken as IScsMessage);
            }
            finally
            {
                Dispose(e);
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void OnReceiveCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                //Get received bytes count
                var bytesRead = e.BytesTransferred;

                if (bytesRead > 0 && e.SocketError == SocketError.Success)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    try
                    {
                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[bytesRead];
                        Array.Copy(e.Buffer, 0, receivedBytes, 0, bytesRead);

                        //Read messages according to current wire protocol
                        var messages = WireProtocol.CreateMessages(receivedBytes);

                        //Raise MessageReceived event for all received messages
                        foreach (var message in messages)
                        {
                            OnMessageReceived(message);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    if (CommunicationState == CommunicationStates.Connected)
                    {
                        //Read more bytes if still running
                        if (!_clientSocket.ReceiveAsync(e))
                        {
                            OnReceiveCompleted(e);
                        }
                    }
                    else
                    {
                        e.SetBuffer(null, 0, 0);
                    }
                }
                else
                {
                    throw new SocketException((int)SocketError.ConnectionReset);
                }
            }
            catch (ObjectDisposedException)
            {
                Dispose(e);
            }
            catch (SocketException ex)
            {
                Disconnect(e);
            }
            catch (Exception ex)
            {
                Dispose(e);

                OnMessageError(ex);
            }
        }

        /// <summary>
        /// Dispose socket async event args.
        /// </summary>
        /// <param name="e"></param>
        private void Dispose(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            try
            {
                e.UserToken = null;
                e.Dispose();
                e = null;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        #endregion
    }
}