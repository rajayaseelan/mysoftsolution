using System;
using System.Net;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Communication.EndPoints;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using System.Threading;
using System.Collections.Generic;
using System.ServiceModel.Channels;

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
        private readonly IPEndPoint _clientEndPoint;

        #endregion

        #region Private fields

        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private readonly Socket _clientSocket;

        private readonly SocketAsyncEventArgsPool _socketPool;

        private readonly BufferManager _bufferManager;

        private readonly int _bufferSize;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        private const int TIMEOUT = 5000;

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
            _clientSocket.SendTimeout = TIMEOUT;
            _clientSocket.ReceiveTimeout = TIMEOUT;
            _clientSocket.NoDelay = true;

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);
            _clientEndPoint = (IPEndPoint)_clientSocket.LocalEndPoint;

            _bufferSize = SocketSetting.BufferSize;
            _socketPool = SocketSetting.TcpSocketPool;

            _bufferManager = BufferManager.CreateBufferManager(0, _bufferSize);

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

            _running = false;
            try
            {
                CommunicationState = CommunicationStates.Disconnected;

                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                if (!_clientSocket.DisconnectAsync(e))
                {
                    AsyncDisconnectComplete(e);
                }

                //_clientSocket.Dispose();
            }
            catch
            {
                OnDisconnected();
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

            //开始异步接收
            BeginAsyncReceive();
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

                //发送数据
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.UserToken = message;
                e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                e.SetBuffer(messageBytes, 0, messageBytes.Length);

                if (!_clientSocket.SendAsync(e))
                {
                    AsyncSendComplete(e);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        private void BeginAsyncReceive()
        {
            if (!_running)
            {
                return;
            }

            if (_socketPool.Count > 0)
            {
                //接收完成
                SocketAsyncEventArgs e = _socketPool.Pop();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                var buffer = _bufferManager.TakeBuffer(_bufferSize);

                e.SetBuffer(buffer, 0, buffer.Length);

                if (!_clientSocket.ReceiveAsync(e))
                {
                    AsyncReceiveComplete(e);
                }
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    {
                        AsyncSendComplete(e);
                        break;
                    }
                case SocketAsyncOperation.Receive:
                    {
                        AsyncReceiveComplete(e);
                        break;
                    }
                case SocketAsyncOperation.Disconnect:
                    {
                        AsyncDisconnectComplete(e);
                        break;
                    }
            }
        }

        void AsyncSendComplete(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    if ((e.Offset + e.BytesTransferred) < e.Count)
                    {
                        //----- Continue to send until all bytes are sent!
                        e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred - e.Offset);

                        if (!_clientSocket.SendAsync(e))
                        {
                            AsyncSendComplete(e);
                        }
                    }
                    else
                    {
                        e.SetBuffer(null, 0, 0);

                        LastSentMessageTime = DateTime.Now;

                        OnMessageSent(e.UserToken as IScsMessage);
                    }
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }
            }
            catch (SocketException ex)
            {
                if ((ex.SocketErrorCode == SocketError.ConnectionReset)
                 || (ex.SocketErrorCode == SocketError.ConnectionAborted)
                 || (ex.SocketErrorCode == SocketError.NotConnected)
                 || (ex.SocketErrorCode == SocketError.Shutdown)
                 || (ex.SocketErrorCode == SocketError.Disconnecting))
                {
                    Disconnect();

                    var error = new CommunicationException(string.Format("Tcp socket ({0}:{1}) is closed.",
                            _clientEndPoint.Address, _clientEndPoint.Port), ex);

                    OnMessageError(error);
                }
                else
                {
                    Console.WriteLine("({0}){1} => {2}", ex.ErrorCode, ex.SocketErrorCode, ex.Message);
                }
            }
            catch (Exception ex)
            {
                OnMessageError(ex);
            }
        }

        /// <summary>
        /// 异步完成
        /// </summary>
        /// <param name="e"></param>
        void AsyncReceiveComplete(SocketAsyncEventArgs e)
        {
            try
            {
                //Get received bytes count
                if (e.SocketError == SocketError.Success)
                {
                    if (e.BytesTransferred > 0)
                    {
                        try
                        {
                            LastReceivedMessageTime = DateTime.Now;

                            //Copy received bytes to a new byte array
                            var receivedBytes = new byte[e.BytesTransferred];
                            Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, e.BytesTransferred);
                            Array.Clear(e.Buffer, 0, e.BytesTransferred);

                            //Read messages according to current wire protocol
                            var messages = WireProtocol.CreateMessages(receivedBytes);

                            //Raise MessageReceived event for all received messages
                            foreach (var message in messages)
                            {
                                OnMessageReceived(message);
                            }

                            //Read more bytes if still running
                            if (_running)
                            {
                                if (!_clientSocket.ReceiveAsync(e))
                                {
                                    AsyncReceiveComplete(e);
                                }
                            }
                        }
                        finally
                        {
                            _bufferManager.ReturnBuffer(e.Buffer);
                        }
                    }
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }
            }
            catch (SocketException ex)
            {
                e.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                _socketPool.Push(e);

                if ((ex.SocketErrorCode == SocketError.ConnectionReset)
                 || (ex.SocketErrorCode == SocketError.ConnectionAborted)
                 || (ex.SocketErrorCode == SocketError.NotConnected)
                 || (ex.SocketErrorCode == SocketError.Shutdown)
                 || (ex.SocketErrorCode == SocketError.Disconnecting))
                {
                    Disconnect();

                    var error = new CommunicationException(string.Format("Tcp socket ({0}:{1}) is closed.",
                            _clientEndPoint.Address, _clientEndPoint.Port), ex);

                    OnMessageError(error);
                }
                else
                {
                    Console.WriteLine("({0}){1} => {2}", ex.ErrorCode, ex.SocketErrorCode, ex.Message);
                }
            }
            catch (Exception ex)
            {
                OnMessageError(ex);
            }
        }

        /// <summary>
        /// 异步完成
        /// </summary>
        /// <param name="e"></param>
        void AsyncDisconnectComplete(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();
                }
            }
            catch
            {

            }

            OnDisconnected();
        }

        #endregion
    }
}
