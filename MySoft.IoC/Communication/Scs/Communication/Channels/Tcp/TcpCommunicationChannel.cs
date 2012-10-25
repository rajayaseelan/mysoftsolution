using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        private volatile ScsTcpEndPoint _remoteEndPoint;

        #endregion

        #region Private fields

        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private volatile Socket _clientSocket;

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 2 * 1024; //2KB

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private volatile byte[] _buffer;

        /// <summary>
        /// The socket is send/receive event args.
        /// </summary>
        private SocketAsyncEventArgs _sendEventArgs, _receiveEventArgs;

        private ManualResetEvent _willRaiseEvent;

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
            _clientSocket.SendBufferSize = BufferSize;
            _clientSocket.ReceiveBufferSize = BufferSize;

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);

            _buffer = new byte[BufferSize];

            //send async event args
            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.Completed += IOCompleted;

            //receive async event args
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.Completed += IOCompleted;

            _willRaiseEvent = new ManualResetEvent(false);
            _syncLock = new object();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (CommunicationState == CommunicationStates.Connected)
            {
                CommunicationState = CommunicationStates.Disconnected;
                OnDisconnected();
            }

            if (_clientSocket != null)
            {
                try
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _clientSocket = null;
                }

                try
                {
                    //Dispose socket event args.
                    _sendEventArgs.Completed -= IOCompleted;
                    _sendEventArgs.UserToken = null;
                    _sendEventArgs.SetBuffer(null, 0, 0);

                    _receiveEventArgs.Completed -= IOCompleted;
                    _receiveEventArgs.UserToken = null;
                    _receiveEventArgs.SetBuffer(null, 0, 0);

                    _sendEventArgs.Dispose();
                    _receiveEventArgs.Dispose();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _sendEventArgs = null;
                    _receiveEventArgs = null;
                }

                try
                {
                    _willRaiseEvent.Close();
                    WireProtocol.Reset();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _buffer = null;
                    _willRaiseEvent = null;
                    _remoteEndPoint = null;
                    WireProtocol = null;
                }
            }
        }

        /// <summary>
        /// Dispose channel
        /// </summary>
        public override void Dispose()
        {
            Disconnect();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            try
            {
                _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);

                if (!_clientSocket.ReceiveAsync(_receiveEventArgs))
                {
                    OnReceiveCompleted(_receiveEventArgs);
                }
            }
            catch (Exception ex)
            {
                Disconnect();

                throw ex;
            }
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                _sendEventArgs.UserToken = message;

                try
                {
                    _sendEventArgs.SetBuffer(messageBytes, 0, messageBytes.Length);

                    //Send all bytes to the remote application
                    if (!_clientSocket.SendAsync(_sendEventArgs))
                    {
                        OnSendCompleted(_sendEventArgs);
                    }

                    //Wait
                    try
                    {
                        if (_willRaiseEvent.WaitOne())
                        {
                            _willRaiseEvent.Reset();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Disconnect();

                    throw ex;
                }
                finally
                {
                    message.Dispose();
                }
            }
        }

        #endregion

        #region Private methods

        private void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void OnSendCompleted(SocketAsyncEventArgs e)
        {
            //Sent success
            var message = e.UserToken as IScsMessage;

            try
            {
                try
                {
                    //Record last sent time
                    LastSentMessageTime = DateTime.Now;

                    OnMessageSent(e.UserToken as IScsMessage);
                }
                finally
                {
                    e.UserToken = null;
                    e.SetBuffer(null, 0, 0);
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                _willRaiseEvent.Set();
            }
            catch (Exception ex)
            {
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
                        Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, bytesRead);

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
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed.");
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        #endregion
    }
}