using MySoft.IoC.Communication.Scs.Communication.EndPoints;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using System;
using System.Net;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    internal class TcpCommunicationChannel : CommunicationChannelBase, ICommunicationProtocol
    {
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

        private bool _fromServer;

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
        /// <param name="fromServer">Message to be sent</param>
        /// used to communicate over network</param>
        public TcpCommunicationChannel(Socket clientSocket, bool fromServer)
        {
            this._clientSocket = clientSocket;
            this._fromServer = fromServer;

            // Disable the Nagle Algorithm for this tcp socket.  
            this._clientSocket.NoDelay = true;
            this._syncLock = new object();

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

            CommunicationState = CommunicationStates.Disconnected;

            try
            {
                //Showdown client socket.
                if (_clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Disconnect(false);
                }

                _clientSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                WireProtocol.Reset();
            }

            //Disconnected
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
            var _receiveEventArgs = CommunicationHelper.Pop(this);

            try
            {
                //Receive all bytes to the remote application
                if (!_clientSocket.ReceiveAsync(_receiveEventArgs))
                {
#if DEBUG
                    IoCHelper.WriteLine(ConsoleColor.DarkGray, "[{0}] {1} receiving message...", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"));
#endif

                    (this as ICommunicationProtocol).Received(_receiveEventArgs);
                }
            }
            catch (Exception ex)
            {
                CommunicationHelper.Push(_receiveEventArgs);

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
            //Send message
            var totalSent = 0;

            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                try
                {
                    //Send all bytes to the remote application
                    while (totalSent < messageBytes.Length)
                    {
                        var sent = _clientSocket.Send(messageBytes, totalSent, messageBytes.Length - totalSent, SocketFlags.None);
                        if (sent <= 0)
                        {
                            throw new CommunicationException("Message could not be sent via TCP socket. Only " + totalSent + " bytes of " + messageBytes.Length + " bytes are sent.");
                        }

                        totalSent += sent;
                    }

                    LastSentMessageTime = DateTime.Now;

                    OnMessageSent(message);
                }
                catch (Exception ex)
                {
                    Disconnect();

                    throw;
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="e">Asyncronous call result</param>
        void ICommunicationProtocol.Received(SocketAsyncEventArgs e)
        {
            try
            {
                //Receive data success.
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Receive buffer data.
                    OnBufferReceived(e.Buffer, e.Offset, e.BytesTransferred);

                    //Receive all bytes to the remote application
                    if (!_clientSocket.ReceiveAsync(e))
                    {
#if DEBUG
                            IoCHelper.WriteLine(ConsoleColor.DarkGray, "[{0}] {1} receiving message...", DateTime.Now, (_fromServer ? "[Server]" : "[Client]"));
#endif

                        (this as ICommunicationProtocol).Received(e);
                    }
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed.");
                }
            }
            catch (Exception ex)
            {
                CommunicationHelper.Push(e);

                Disconnect();
            }
        }

        /// <summary>
        /// Received data buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="bytesTransferred"></param>
        private void OnBufferReceived(byte[] buffer, int offset, int bytesTransferred)
        {
            //Copy received bytes to a new byte array
            var receivedBytes = new byte[bytesTransferred];

            //Copy buffer.
            Buffer.BlockCopy(buffer, offset, receivedBytes, 0, bytesTransferred);

            //Read messages according to current wire protocol
            var messages = WireProtocol.CreateMessages(receivedBytes);

            //Raise MessageReceived event for all received messages
            foreach (var message in messages)
            {
                OnMessageReceived(message);
            }
        }

        #endregion
    }
}