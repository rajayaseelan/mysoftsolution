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
            _clientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            var endPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
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

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();

            try
            {
                WireProtocol.Reset();

                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch
            {
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            //Create receive event args.
            var _receiveEventArgs = PopSocketEventArgs(_buffer, null);

            SendOrReceiveBufferData(_receiveEventArgs, true);
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            //Create a byte array from message according to current protocol
            var messageBytes = WireProtocol.GetBytes(message);

            var userToken = new DataHoldingUserToken(message, messageBytes);
            var buffer = userToken.GetRemainingBuffer(BufferSize);

            //设置缓冲区
            var _sendEventArgs = PopSocketEventArgs(buffer, userToken);

            //发送缓冲区数据
            SendOrReceiveBufferData(_sendEventArgs, false);
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
        /// 发送或接收缓冲区数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="received"></param>
        private void SendOrReceiveBufferData(SocketAsyncEventArgs e, bool received)
        {
            try
            {
                if (received)
                {
                    //Receive all bytes to the remote application
                    if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        OnReceiveCompleted(e);
                    }
                }
                else
                {
                    //Send all bytes to the remote application
                    if (!e.AcceptSocket.SendAsync(e))
                    {
                        OnSendCompleted(e);
                    }
                }
            }
            catch (Exception ex)
            {
                PushSocketEventArgs(e);

                Disconnect();

                throw;
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void OnSendCompleted(SocketAsyncEventArgs e)
        {
            var userToken = e.UserToken as DataHoldingUserToken;

            try
            {
                var buffer = userToken.GetRemainingBuffer(BufferSize);

                if (buffer == null)
                {
                    //Record last sent time
                    LastSentMessageTime = DateTime.Now;

                    OnMessageSent(userToken.Message);
                }
                else
                {
                    //设置缓冲区
                    var _sendEventArgs = PopSocketEventArgs(buffer, userToken);

                    //发送缓冲区数据
                    SendOrReceiveBufferData(_sendEventArgs, false);
                }
            }
            catch (Exception ex) { }
            finally
            {
                PushSocketEventArgs(e);
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
                var bytesTransferred = e.BytesTransferred;

                if (e.SocketError == SocketError.Success && bytesTransferred > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, bytesTransferred);
                    Array.Clear(e.Buffer, 0, bytesTransferred);

                    //Read messages according to current wire protocol
                    var messages = WireProtocol.CreateMessages(receivedBytes);

                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        OnMessageReceived(message);
                    }

                    //设置缓冲区
                    var _receiveEventArgs = PopSocketEventArgs(_buffer, null);

                    //重新发送缓存数据
                    SendOrReceiveBufferData(_receiveEventArgs, true);
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
            finally
            {
                PushSocketEventArgs(e);
            }
        }

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private SocketAsyncEventArgs PopSocketEventArgs(byte[] buffer, object token)
        {
            var e = TcpCommunicationHelper.Pop();
            e.Completed += IOCompleted;
            e.SetBuffer(buffer, 0, buffer.Length);
            e.AcceptSocket = _clientSocket;
            e.UserToken = token;

            return e;
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void PushSocketEventArgs(SocketAsyncEventArgs e)
        {
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
                TcpCommunicationHelper.Push(e);
            }
        }
    }
}