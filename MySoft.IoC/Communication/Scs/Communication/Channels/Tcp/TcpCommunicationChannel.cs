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

        // Socket send / receive timeout.
        const int SocketTimeout = 5 * 1000;

        // Socket send / receive timeout.
        const int SocketBufferSize = 16 * 1024; //16kb

        //create byte array to store: ensure at least 1 byte!
        const int BufferSize = 4 * 1024; //4kb

        private readonly byte[] _buffer;

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
        public TcpCommunicationChannel(Socket clientSocket, bool sendByServer)
        {
            _clientSocket = clientSocket;
            _clientSocket.SendTimeout = SocketTimeout;
            _clientSocket.ReceiveTimeout = SocketTimeout;
            _clientSocket.SendBufferSize = SocketBufferSize;
            _clientSocket.ReceiveBufferSize = SocketBufferSize;
            _clientSocket.NoDelay = true;

            var endPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(endPoint.Address.ToString(), endPoint.Port);

            _buffer = new byte[BufferSize];
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
            CommunicationState = CommunicationStates.Disconnected;

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            try
            {
                if (!_clientSocket.DisconnectAsync(e))
                {
                    AsyncDisconnectComplete(e);
                }

                //_clientSocket.Dispose();
            }
            catch
            {
                TcpSocketHelper.Dispose(e);

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

            //Create a byte array from message according to current protocol
            var messageBytes = WireProtocol.GetBytes(message);

            //发送数据
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.UserToken = message;
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

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
                TcpSocketHelper.Dispose(e);

                throw ex;
            }
        }

        #region 转换成byte组

        /// <summary>
        /// 转换成byte组
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        //private IList<ArraySegment<byte>> ConvertBufferList(byte[] source, int size)
        //{
        //    var target = new List<ArraySegment<byte>>();
        //    int l = source.Length / size;
        //    int y = source.Length % size;

        //    for (int i = 0; i < l; i++)
        //    {
        //        target.Add(new ArraySegment<byte>(source, i * size, size));
        //    }

        //    if (y != 0)
        //    {
        //        target.Add(new ArraySegment<byte>(source, l * size, y));
        //    }

        //    return target;
        //}

        #endregion

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
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            try
            {
                //clear buffer
                if (_buffer != null)
                    Array.Clear(_buffer, 0, _buffer.Length);

                e.SetBuffer(_buffer, 0, _buffer.Length);

                if (!_clientSocket.ReceiveAsync(e))
                {
                    AsyncReceiveComplete(e);
                }
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Dispose(e);

                throw ex;
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
                        if (e.Buffer != null)
                            Array.Clear(e.Buffer, 0, e.Count);

                        LastSentMessageTime = DateTime.Now;

                        OnMessageSent(e.UserToken as IScsMessage);

                        TcpSocketHelper.Dispose(e);
                    }
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }
            }
            catch (SocketException ex)
            {
                TcpSocketHelper.Dispose(e);

                if ((ex.SocketErrorCode == SocketError.ConnectionReset)
                 || (ex.SocketErrorCode == SocketError.ConnectionAborted)
                 || (ex.SocketErrorCode == SocketError.NotConnected)
                 || (ex.SocketErrorCode == SocketError.Shutdown)
                 || (ex.SocketErrorCode == SocketError.Disconnecting))
                {
                    Disconnect();

                    var error = new CommunicationException(string.Format("Tcp socket (local:{0} => remote:{1}) is closed.",
                                    _clientSocket.RemoteEndPoint, _clientSocket.LocalEndPoint), ex);

                    OnMessageError(error);
                }
                else
                {
                    Console.WriteLine("({0}){1} => {2}", ex.ErrorCode, ex.SocketErrorCode, ex.Message);

                    OnMessageError(ex);
                }
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Dispose(e);

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
                        LastReceivedMessageTime = DateTime.Now;

                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, e.BytesTransferred);

                        if (e.Buffer != null)
                            Array.Clear(e.Buffer, 0, e.BytesTransferred);

                        //Read messages according to current wire protocol
                        var messages = WireProtocol.CreateMessages(receivedBytes);

                        receivedBytes = null;

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
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }
            }
            catch (SocketException ex)
            {
                TcpSocketHelper.Dispose(e);

                if ((ex.SocketErrorCode == SocketError.ConnectionReset)
                 || (ex.SocketErrorCode == SocketError.ConnectionAborted)
                 || (ex.SocketErrorCode == SocketError.NotConnected)
                 || (ex.SocketErrorCode == SocketError.Shutdown)
                 || (ex.SocketErrorCode == SocketError.Disconnecting))
                {
                    Disconnect();

                    var error = new CommunicationException(string.Format("Tcp socket ({0} => {1}) is closed.",
                                    _clientSocket.RemoteEndPoint, _clientSocket.LocalEndPoint), ex);

                    OnMessageError(error);
                }
                else
                {
                    Console.WriteLine("({0}){1} => {2}", ex.ErrorCode, ex.SocketErrorCode, ex.Message);

                    OnMessageError(ex);
                }
            }
            catch (Exception ex)
            {
                TcpSocketHelper.Dispose(e);

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
            finally
            {
                TcpSocketHelper.Dispose(e);
            }

            OnDisconnected();
        }

        #endregion
    }
}
