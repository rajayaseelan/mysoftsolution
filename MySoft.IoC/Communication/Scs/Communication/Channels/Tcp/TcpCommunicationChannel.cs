using System;
using System.Net;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Communication.EndPoints;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using System.Collections.Generic;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    internal class TcpCommunicationChannel : CommunicationChannelBase, ITcpChannel
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

        private readonly TcpSocketAsyncEventArgsPool _pool;

        //create byte array to store: ensure at least 1 byte!
        const int BufferSize = 2 * 1024; //2kb

        const int SocketTimeout = 5 * 60 * 1000; //timeout 5 minutes

        private readonly byte[] _buffer;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private static readonly object _syncLock = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="clientSocket">A connected Socket object that is
        /// used to communicate over network</param>
        public TcpCommunicationChannel(Socket clientSocket, bool sendByServer)
        {
            _clientSocket = clientSocket;
            _clientSocket.NoDelay = true;
            _clientSocket.SendTimeout = SocketTimeout;
            _clientSocket.ReceiveTimeout = SocketTimeout;

            var endPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(endPoint.Address.ToString(), endPoint.Port);

            _pool = TcpSocketSetting.SocketPool;
            _buffer = new byte[BufferSize];
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

            TcpSocketAsyncEventArgs e = _pool.Pop(this);

            try
            {
                if (!_clientSocket.DisconnectAsync(e))
                {
                    AsyncDisconnectComplete(e);
                }

                //_clientSocket.Release();
            }
            catch
            {
                TcpSocketHelper.Release(e);

                lock (_syncLock)
                {
                    OnDisconnected();
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
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                //发送数据
                TcpSocketAsyncEventArgs e = _pool.Pop(this);
                e.UserToken = message;

                try
                {
                    e.SetBuffer(messageBytes, 0, messageBytes.Length);

                    if (!_clientSocket.SendAsync(e))
                    {
                        AsyncSendComplete(e);
                    }
                }
                catch (Exception ex)
                {
                    TcpSocketHelper.Release(e);

                    throw ex;
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

            //接收数据
            TcpSocketAsyncEventArgs e = _pool.Pop(this);

            try
            {
                e.SetBuffer(_buffer, 0, _buffer.Length);

                if (!_clientSocket.ReceiveAsync(e))
                {
                    AsyncReceiveComplete(e);
                }
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Release(e);

                throw ex;
            }
        }

        void AsyncSendComplete(TcpSocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    LastSentMessageTime = DateTime.Now;

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
                        var message = e.UserToken as IScsMessage;
                        OnMessageSent(message);

                        TcpSocketHelper.Release(e);
                    }
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }
            }
            catch (SocketException ex)
            {
                TcpSocketHelper.Release(e);

                if ((ex.SocketErrorCode == SocketError.ConnectionReset)
                     || (ex.SocketErrorCode == SocketError.ConnectionAborted)
                     || (ex.SocketErrorCode == SocketError.NotConnected)
                     || (ex.SocketErrorCode == SocketError.Shutdown)
                     || (ex.SocketErrorCode == SocketError.Disconnecting)
                     || (ex.SocketErrorCode == SocketError.OperationAborted))
                {
                    Disconnect();
                }
                else
                {
                    OnMessageError(ex);
                }
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Release(e);

                OnMessageError(ex);
            }
        }

        /// <summary>
        /// 异步完成
        /// </summary>
        /// <param name="e"></param>
        void AsyncReceiveComplete(TcpSocketAsyncEventArgs e)
        {
            try
            {
                //Get received bytes count
                if (e.SocketError == SocketError.Success)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    if (e.BytesTransferred > 0)
                    {
                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, e.BytesTransferred);

                        //Read messages according to current wire protocol
                        var messages = WireProtocol.CreateMessages(receivedBytes);

                        //Raise MessageReceived event for all received messages
                        foreach (var message in messages)
                        {
                            OnMessageReceived(message);
                        }
                    }

                    if (!_running)
                    {
                        //释放资源
                        TcpSocketHelper.Release(e);
                    }
                    else
                    {
                        //重新接收数据
                        if (!_clientSocket.ReceiveAsync(e))
                        {
                            AsyncReceiveComplete(e);
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
                TcpSocketHelper.Release(e);

                if ((ex.SocketErrorCode == SocketError.ConnectionReset)
                     || (ex.SocketErrorCode == SocketError.ConnectionAborted)
                     || (ex.SocketErrorCode == SocketError.NotConnected)
                     || (ex.SocketErrorCode == SocketError.Shutdown)
                     || (ex.SocketErrorCode == SocketError.Disconnecting)
                     || (ex.SocketErrorCode == SocketError.OperationAborted))
                {
                    Disconnect();
                }
                else
                {
                    OnMessageError(ex);
                }
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Release(e);

                OnMessageError(ex);
            }
        }

        /// <summary>
        /// 异步完成
        /// </summary>
        /// <param name="e"></param>
        void AsyncDisconnectComplete(TcpSocketAsyncEventArgs e)
        {
            try
            {
                //_clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch
            {

            }
            finally
            {
                //Dispose resource.
                TcpSocketHelper.Release(e);
            }

            lock (_syncLock)
            {
                OnDisconnected();
            }
        }

        #endregion

        #region ITcpChannel 成员

        /// <summary>
        /// 接收完成
        /// </summary>
        /// <param name="e"></param>
        public void OnReceiveComplete(SocketAsyncEventArgs e)
        {
            AsyncReceiveComplete(e as TcpSocketAsyncEventArgs);
        }

        /// <summary>
        /// 发送完成
        /// </summary>
        /// <param name="e"></param>
        public void OnSendComplete(SocketAsyncEventArgs e)
        {
            AsyncSendComplete(e as TcpSocketAsyncEventArgs);
        }

        /// <summary>
        /// 断开完成
        /// </summary>
        /// <param name="e"></param>
        public void OnDisconnectComplete(SocketAsyncEventArgs e)
        {
            AsyncDisconnectComplete(e as TcpSocketAsyncEventArgs);
        }

        #endregion
    }
}
