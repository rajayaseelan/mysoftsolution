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
    internal class TcpCommunicationChannel : CommunicationChannelBase, ICommunicationProtocol
    {
        /// <summary>
        /// Size of the buffer that is used to send bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 2 * 1024; //2KB

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

        private readonly bool _fromServer;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private volatile byte[] _receiveBuffer;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="clientSocket">A connected Socket object that is
        /// used to communicate over network</param>
        public TcpCommunicationChannel(Socket clientSocket, bool fromServer)
        {
            this._clientSocket = clientSocket;
            this._fromServer = fromServer;

            // Disable the Nagle Algorithm for this tcp socket.  
            this._clientSocket.NoDelay = true;

            this._receiveBuffer = new byte[ReceiveBufferSize];

            var endPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
            this._remoteEndPoint = new ScsTcpEndPoint(endPoint.Address.ToString(), endPoint.Port);
        }

        #endregion

        #region Public methods

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
                //Showdown client socket.
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                WireProtocol.Reset();
            }

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
            _running = true;

            //Socket receive messages event args.
            var _receiveEventArgs = CreateAsyncSEA(_receiveBuffer, null);

            try
            {
                //Receive all bytes to the remote application
                if (!_clientSocket.ReceiveAsync(_receiveEventArgs))
                {
#if DEBUG
                    IoCHelper.WriteLine(ConsoleColor.DarkGray, "[{0}] {1} receiving message...", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"));
#endif

                    (this as ICommunicationProtocol).ReceiveCompleted(_receiveEventArgs);
                }
            }
            catch (Exception ex)
            {
                DisposeAsyncSEA(_receiveEventArgs);

                Disconnect();

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

            //Data packet size.
            message.DataLength = messageBytes.Length;

            //Socket send messages event args.
            var _sendEventArgs = CreateAsyncSEA(messageBytes, message);

            try
            {
                //Send all bytes to the remote application
                if (!_clientSocket.SendAsync(_sendEventArgs))
                {
#if DEBUG
                    IoCHelper.WriteLine(ConsoleColor.DarkGray, "[{0}] {1} sending message...", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"));
#endif

                    (this as ICommunicationProtocol).SendCompleted(_sendEventArgs);
                }
            }
            catch (Exception ex)
            {
                DisposeAsyncSEA(_sendEventArgs);

                Disconnect();

                throw;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        void ICommunicationProtocol.SendCompleted(SocketAsyncEventArgs e)
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
                DisposeAsyncSEA(e);
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="e">Asyncronous call result</param>
        void ICommunicationProtocol.ReceiveCompleted(SocketAsyncEventArgs e)
        {
            if (!_running)
            {
                DisposeAsyncSEA(e);

                return;
            }

            try
            {
                //Receive data success.
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    try
                    {
                        //处理接收的缓冲数据
                        OnBufferReceived(e.Buffer, e.BytesTransferred);
                    }
                    catch (Exception ex)
                    {
                        OnMessageError(ex);

                        throw;
                    }

                    if (_running)
                    {
                        //Receive all bytes to the remote application
                        if (!_clientSocket.ReceiveAsync(e))
                        {
#if DEBUG
                            IoCHelper.WriteLine(ConsoleColor.DarkGray, "[{0}] {1} receiving message...", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"));
#endif

                            (this as ICommunicationProtocol).ReceiveCompleted(e);
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
                DisposeAsyncSEA(e);

                Disconnect();
            }
        }

        /// <summary>
        /// Received data buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesTransferred"></param>
        private void OnBufferReceived(byte[] buffer, int bytesTransferred)
        {
            //Copy received bytes to a new byte array
            var receivedBytes = new byte[bytesTransferred];

            //Copy buffer.
            Buffer.BlockCopy(buffer, 0, receivedBytes, 0, bytesTransferred);

            //Read messages according to current wire protocol
            var messages = WireProtocol.CreateMessages(receivedBytes);

            //Raise MessageReceived event for all received messages
            foreach (var message in messages)
            {
                OnMessageReceived(message);
            }
        }

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateAsyncSEA(byte[] buffer, object token)
        {
            var e = CommunicationHelper.Pop(this);

#if DEBUG
            IoCHelper.WriteLine(ConsoleColor.DarkCyan, "[{0}] {1} pop tcp socket event async count: {2}", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"), CommunicationHelper.Count);
#endif

            if (e == null) return null;

            e.AcceptSocket = _clientSocket;
            if (token != null)
            {
                e.UserToken = token;
            }
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
        private void DisposeAsyncSEA(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            try
            {
                e.AcceptSocket = null;
                e.UserToken = null;
                e.SetBuffer(null, 0, 0);
            }
            catch (Exception ex) { }
            finally
            {
                CommunicationHelper.Push(e);
            }

#if DEBUG
            IoCHelper.WriteLine(ConsoleColor.DarkRed, "[{0}] {1} push tcp socket event async count: {2}", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"), CommunicationHelper.Count);
#endif
        }
    }
}