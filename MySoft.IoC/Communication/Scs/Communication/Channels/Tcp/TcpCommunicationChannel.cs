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
        private static readonly object syncRoot = new object();

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
        private SendMessageQueue _sendQueue;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        //private readonly object _syncLock;

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private byte[] _receiveBuffer;

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
        public TcpCommunicationChannel(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _clientSocket.NoDelay = true;

            var endPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(endPoint.Address.ToString(), endPoint.Port);

            _receiveBuffer = new byte[ReceiveBufferSize];

            //Socket send messages event args.
            var _sendEventArgs = CreateAsyncSEA(null);

            _sendQueue = new SendMessageQueue(clientSocket, _sendEventArgs);
            _sendQueue.Completed += (sender, e) => OnSendCompleted(e);
            _sendQueue.Disposed += (sender, e) => DisposeAsyncSEA(e);
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
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception ex) { }
            finally
            {
                WireProtocol.Reset();
                _sendQueue.Dispose();

                _sendQueue = null;
                _receiveBuffer = null;
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
            var _receiveEventArgs = CreateAsyncSEA(_receiveBuffer);

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

            try
            {
                //Send all bytes to the remote application
                _sendQueue.Send(message, messageBytes);
            }
            catch (Exception ex)
            {
                Disconnect();

                throw;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// IO回调处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ICommunicationProtocol.IOCompleted(SocketAsyncEventArgs e)
        {
            try
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
            try
            {
                //Record last sent time
                LastSentMessageTime = DateTime.Now;

                OnMessageSent(e.UserToken as IScsMessage);
            }
            catch (Exception ex) { }
            finally
            {
                try
                {
                    //继续发送
                    _sendQueue.ContinueSend(e);
                }
                catch (Exception ex)
                {
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="e">Asyncronous call result</param>
        private void OnReceiveCompleted(SocketAsyncEventArgs e)
        {
            if (!_running)
            {
                DisposeAsyncSEA(e);

                return;
            }

            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    //Receive data success.
                    if (e.BytesTransferred > 0)
                    {
                        LastReceivedMessageTime = DateTime.Now;

                        try
                        {
                            //Copy received bytes to a new byte array
                            var receivedBytes = new byte[e.BytesTransferred];

                            //Copy buffer.
                            Buffer.BlockCopy(e.Buffer, 0, receivedBytes, 0, e.BytesTransferred);

                            //Clear buffer.
                            Array.Clear(e.Buffer, 0, e.BytesTransferred);

                            //Read messages according to current wire protocol
                            var messages = WireProtocol.CreateMessages(receivedBytes);

                            //Raise MessageReceived event for all received messages
                            foreach (var message in messages)
                            {
                                OnMessageReceived(message);
                            }
                        }
                        catch (Exception ex) //Catch Exception
                        {
                            //Show error.
                            OnMessageError(ex);

                            throw;
                        }
                        finally
                        {
                            //设置偏移量
                            e.SetBuffer(0, e.Count);

                            //Receive all bytes to the remote application
                            if (!_clientSocket.ReceiveAsync(e))
                            {
                                OnReceiveCompleted(e);
                            }
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

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateAsyncSEA(byte[] buffer)
        {
            lock (syncRoot)
            {
                var e = CommunicationHelper.Pop(this);

                e.AcceptSocket = _clientSocket;
                e.UserToken = _clientSocket;

                if (buffer != null)
                {
                    e.SetBuffer(buffer, 0, buffer.Length);
                }

                return e;
            }
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void DisposeAsyncSEA(SocketAsyncEventArgs e)
        {
            lock (syncRoot)
            {
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
            }
        }
    }
}