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
        /// <summary>
        /// Size of the buffer that is used to send bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 4 * 1024; //4KB

        #region Public properties

        private readonly ScsEndPoint _remoteEndPoint;

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

        #endregion

        #region Private fields

        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private readonly Socket _clientSocket;

        /// <summary>
        /// Socket send messages event args.
        /// </summary>
        private SocketAsyncEventArgs _sendEventArgs;
        private SocketAsyncEventArgs _receiveEventArgs;

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private byte[] _receiveBuffer;

        /// <summary>
        /// Send or receive event args.
        /// </summary>
        private SendMessageQueue _sendQueue;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

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

            var endPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(endPoint.Address.ToString(), endPoint.Port);

            _receiveBuffer = new byte[ReceiveBufferSize];

            _sendEventArgs = CreateSocketEventArgs(null);
            _receiveEventArgs = CreateSocketEventArgs(_receiveBuffer);

            _sendQueue = new SendMessageQueue(_clientSocket, _sendEventArgs);
            _sendQueue.Completed += IOCompleted;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        /// <param name="ex"></param>
        private void Disconnect(Exception ex)
        {
            if (!(ex is CommunicationException || ex is SocketException))
            {
                OnMessageError(ex);
            }

            //Disconnect server.
            Disconnect();
        }

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (!_running)
            {
                return;
            }

            _running = false;

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                Dispose();
            }

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        /// <summary>
        /// Dispose resource.
        /// </summary>
        private void Dispose()
        {
            try
            {
                DisposeSocketEventArgs(_sendEventArgs);
                DisposeSocketEventArgs(_receiveEventArgs);
            }
            catch (Exception ex) { }
            finally
            {
                try
                {
                    WireProtocol.Reset();

                    _sendQueue.Completed -= IOCompleted;
                    _sendQueue.Dispose();
                }
                catch (Exception ex) { }
                finally
                {
                    _sendQueue = null;
                    _receiveBuffer = null;
                    WireProtocol = null;

                    _sendEventArgs = null;
                    _receiveEventArgs = null;
                }
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            _running = true;

            try
            {
                //Receive all bytes to the remote application
                if (!_clientSocket.ReceiveAsync(_receiveEventArgs))
                {
                    OnReceiveCompleted(_receiveEventArgs);
                }
            }
            catch (Exception ex)
            {
                Disconnect(ex);

                throw;
            }
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            if (!_running)
            {
                return;
            }

            //Create a byte array from message according to current protocol
            var messageBytes = WireProtocol.GetBytes(message);

            try
            {
                _sendQueue.Send(message, messageBytes);
            }
            catch (Exception ex)
            {
                Disconnect(ex);

                throw;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// IO回调处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    OnSendCompleted(e);
                    break;
                case SocketAsyncOperation.Receive:
                    OnReceiveCompleted(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
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

                OnMessageSent(e.UserToken as IScsMessage);
            }
            catch (Exception ex) { }
            finally
            {
                _sendQueue.Reset(e);
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void OnReceiveCompleted(SocketAsyncEventArgs e)
        {
            if (!_running)
            {
                return;
            }

            try
            {
                //Receive data success.
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Get received bytes count
                    var bytesTransferred = e.BytesTransferred;

                    if (bytesTransferred > 0)
                    {
                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[bytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, bytesTransferred);
                        //Array.Clear(e.Buffer, 0, bytesTransferred);

                        //Read messages according to current wire protocol
                        var messages = WireProtocol.CreateMessages(receivedBytes);

                        //Raise MessageReceived event for all received messages
                        foreach (var message in messages)
                        {
                            OnMessageReceived(message);
                        }

                        //Receive all bytes to the remote application
                        if (!_clientSocket.ReceiveAsync(e))
                        {
                            OnReceiveCompleted(e);
                        }
                    }
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed.");
                }
            }
            catch (Exception ex)
            {
                Disconnect(ex);
            }
        }

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateSocketEventArgs(byte[] buffer)
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += IOCompleted;

            if (buffer != null)
            {
                e.SetBuffer(buffer, 0, buffer.Length);
            }

            return e;
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void DisposeSocketEventArgs(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            try
            {
                e.Completed -= IOCompleted;
                e.SetBuffer(null, 0, 0);
                e.AcceptSocket = null;
                e.UserToken = null;
            }
            catch (Exception ex) { }
            finally
            {
                e.Dispose();
            }
        }
    }
}