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
        private readonly Socket _clientSocket;

        private readonly ManualResetEvent _willRaiseEvent;

        /// <summary>
        /// Socket object async args.
        /// </summary>
        private readonly SocketAsyncEventArgs _sendEventArgs;
        private readonly SocketAsyncEventArgs _receiveEventArgs;

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 2 * 1024; //2KB

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private volatile byte[] _buffer;

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

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);

            _buffer = new byte[BufferSize];

            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

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
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                _sendEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                _receiveEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(IOCompleted);

                _sendEventArgs.SetBuffer(null, 0, 0);
                _receiveEventArgs.SetBuffer(null, 0, 0);
            }
            catch (Exception ex) { }
            finally
            {
                _sendEventArgs.Dispose();
                _receiveEventArgs.Dispose();
            }

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                _willRaiseEvent.Close();
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

                throw;
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

                _willRaiseEvent.Reset();

                try
                {
                    _sendEventArgs.SetBuffer(messageBytes, 0, messageBytes.Length);
                    _sendEventArgs.UserToken = message;

                    //Send all bytes to the remote application
                    if (!_clientSocket.SendAsync(_sendEventArgs))
                    {
                        OnSendCompleted(_sendEventArgs);
                    }

                    //Wait
                    _willRaiseEvent.WaitOne();
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
        /// IO回调处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            else
            {
                Disconnect();
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
                //Record last sent time
                LastSentMessageTime = DateTime.Now;

                OnMessageSent(e.UserToken as IScsMessage);
            }
            catch (Exception ex) { }
            finally
            {
                e.SetBuffer(null, 0, 0);
            }

            _willRaiseEvent.Set();
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
                        OnMessageError(ex);
                    }

                    if (!_clientSocket.ReceiveAsync(_receiveEventArgs))
                    {
                        OnReceiveCompleted(_receiveEventArgs);
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