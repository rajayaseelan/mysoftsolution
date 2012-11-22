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
    internal class TcpCommunicationChannel : CommunicationChannelBase, ITcpSocketChannel
    {
        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 2 * 1024; //2KB

        /// <summary>
        /// Connection of the socket that is used to receive bytes from TCP socket.
        /// </summary>
        private const int MaxConnection = 1000; //Max connection

        private static readonly TcpSocketAsyncEventArgsPool pool;

        static TcpCommunicationChannel()
        {
            pool = InitSocketPool(MaxConnection);
        }

        /// <summary>
        /// 初始化Socket池
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        internal static TcpSocketAsyncEventArgsPool InitSocketPool(int capacity)
        {
            var pool = new TcpSocketAsyncEventArgsPool(capacity);

            for (int i = 0; i < capacity; i++)
            {
                var e = new TcpSocketAsyncEventArgs();
                pool.Push(e);
            }

            return pool;
        }

        #region Public properties

        private volatile ScsTcpEndPoint _remoteEndPoint;

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
        private Socket _clientSocket;

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private byte[] _buffer;

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
            _clientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);

            _buffer = new byte[BufferSize];
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

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex) { }
            finally
            {
                try
                {
                    WireProtocol.Reset();
                    _clientSocket.Close();
                }
                catch
                {
                }

                _clientSocket = null;
                _buffer = null;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            lock (_syncLock)
            {
                var e = PopSocketEventArgs();

                try
                {
                    e.SetBuffer(_buffer, 0, _buffer.Length);

                    if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        (this as ITcpSocketChannel).OnReceiveCompleted(e);
                    }
                }
                catch (Exception ex)
                {
                    PushSocketEventArgs(e);

                    Disconnect();

                    throw;
                }
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

                var e = PopSocketEventArgs();

                try
                {
                    e.SetBuffer(messageBytes, 0, messageBytes.Length);
                    e.UserToken = message;

                    //Send all bytes to the remote application
                    if (!e.AcceptSocket.SendAsync(e))
                    {
                        (this as ITcpSocketChannel).OnSendCompleted(e);
                    }
                }
                catch (Exception ex)
                {
                    PushSocketEventArgs(e);

                    Disconnect();
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        void ITcpSocketChannel.OnSendCompleted(SocketAsyncEventArgs e)
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
                PushSocketEventArgs(e);
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        void ITcpSocketChannel.OnReceiveCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                //Get received bytes count
                var bytesTransferred = e.BytesTransferred;

                if (bytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    try
                    {
                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[bytesTransferred];
                        Buffer.BlockCopy(_buffer, 0, receivedBytes, 0, bytesTransferred);
                        Array.Clear(_buffer, 0, BufferSize);

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

                        throw;
                    }

                    //重新开始接收数据
                    if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        (this as ITcpSocketChannel).OnReceiveCompleted(e);
                    }
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed.");
                }
            }
            catch (Exception ex)
            {
                PushSocketEventArgs(e);

                Disconnect();
            }
        }

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs PopSocketEventArgs()
        {
            var e = pool.Pop();
            if (e == null)
            {
                e = new TcpSocketAsyncEventArgs();
            }

            e.AcceptSocket = _clientSocket;
            e.Channel = this;

            return e;
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void PushSocketEventArgs(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            if (e is TcpSocketAsyncEventArgs)
            {
                var te = e as TcpSocketAsyncEventArgs;
                te.SetBuffer(null, 0, 0);
                te.AcceptSocket = null;
                te.UserToken = null;
                te.Channel = null;

                pool.Push(te);
            }
        }
    }
}