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
    internal class TcpCommunicationChannel : CommunicationChannelBase, ICommunicationCompleted
    {
        /// <summary>
        /// Size of the buffer that is used to send bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 4 * 1024; //4KB

        #region Public properties

        private ScsEndPoint _remoteEndPoint;

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
        /// This buffer is used to receive bytes 
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Send or receive event args.
        /// </summary>
        private readonly SendMessageQueue _sendQueue;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

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
            _clientSocket.UseOnlyOverlappedIO = false;

            var endPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(endPoint.Address.ToString(), endPoint.Port);

            _sendQueue = new SendMessageQueue(_clientSocket);

            _buffer = new byte[BufferSize];
            _syncLock = new object();
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

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                WireProtocol.Reset();
                _sendQueue.Dispose();
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

            var _receiveEventArgs = PopSocketEventArgs(_buffer);

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
                DisposeSocketEventArgs(_receiveEventArgs);

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

            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                var userMessage = new MessageUserToken(message, messageBytes);

                var _sendEventArgs = PopSocketEventArgs(null);

                try
                {
                    _sendQueue.SendMessage(userMessage, _sendEventArgs);
                }
                catch (Exception ex)
                {
                    DisposeSocketEventArgs(_sendEventArgs);

                    Disconnect(ex);

                    throw;
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// IO回调处理
        /// </summary>
        /// <param name="e"></param>
        void ICommunicationCompleted.IOCompleted(SocketAsyncEventArgs e)
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

                var message = e.UserToken as MessageUserToken;
                OnMessageSent(message.Message);
            }
            catch (Exception ex) { }
            finally
            {
                e.UserToken = null;
                e.SetBuffer(null, 0, 0);

                //重置状态
                _sendQueue.Reset();
            }

            try
            {
                _sendQueue.SendMessage(e);
            }
            catch (Exception ex)
            {
                //Dispose socket event args.
                DisposeSocketEventArgs(e);

                Disconnect(ex);
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
                //Get received bytes count
                var bytesTransferred = e.BytesTransferred;

                if (e.SocketError == SocketError.Success && bytesTransferred > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, bytesTransferred);

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
                else
                {
                    throw new CommunicationException("Tcp socket is closed.");
                }
            }
            catch (Exception ex)
            {
                //Dispose socket event args.
                DisposeSocketEventArgs(e);

                Disconnect(ex);
            }
        }

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private SocketAsyncEventArgs PopSocketEventArgs(byte[] buffer)
        {
            var e = new TcpSocketAsyncEventArgs();
            e.Channel = this;

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
                e.SetBuffer(null, 0, 0);
                e.UserToken = null;

                var tcp = e as TcpSocketAsyncEventArgs;
                tcp.Channel = null;
            }
            catch (Exception ex) { }
            finally
            {
                e.Dispose();
            }
        }
    }
}