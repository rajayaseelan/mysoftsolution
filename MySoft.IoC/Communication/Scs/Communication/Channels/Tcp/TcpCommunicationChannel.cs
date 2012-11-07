using System;
using System.Net;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Communication.EndPoints;
using MySoft.IoC.Communication.Scs.Communication.EndPoints.Tcp;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.Threading;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    internal class TcpCommunicationChannel : CommunicationChannelBase
    {
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
        private readonly Socket _clientSocket;

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 2 * 1024; //2KB

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
            _clientSocket.NoDelay = true;

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);

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

            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex) { }
            finally
            {
                _clientSocket.Close();
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
            var e = NewSocketEventArgs();

            try
            {
                e.SetBuffer(_buffer, 0, _buffer.Length);

                if (!_clientSocket.ReceiveAsync(e))
                {
                    OnReceiveCompleted(e);
                }
            }
            catch (Exception ex)
            {
                //Dispose event args.
                DisposeSocketEventArgs(e);

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
            //Create a byte array from message according to current protocol
            var messageBytes = WireProtocol.GetBytes(message);

            var e = NewSocketEventArgs();

            try
            {
                e.SetBuffer(messageBytes, 0, messageBytes.Length);
                e.UserToken = message;

                //Send all bytes to the remote application
                if (!_clientSocket.SendAsync(e))
                {
                    OnSendCompleted(e);
                }
            }
            catch (Exception ex)
            {
                //Dispose event args.
                DisposeSocketEventArgs(e);

                Disconnect();
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
            catch
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
                //Record last sent time
                LastSentMessageTime = DateTime.Now;

                OnMessageSent(e.UserToken as IScsMessage);
            }
            catch (Exception ex) { }
            finally
            {
                //Dispose event args.
                DisposeSocketEventArgs(e);
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

                if (bytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    try
                    {
                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[bytesTransferred];
                        Buffer.BlockCopy(e.Buffer, e.Offset, receivedBytes, 0, bytesTransferred);
                        Array.Clear(e.Buffer, e.Offset, bytesTransferred);

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

                    if (!_clientSocket.ReceiveAsync(e))
                    {
                        OnReceiveCompleted(e);
                    }
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed.");
                }
            }
            catch (Exception ex)
            {
                //Dispose event args.
                DisposeSocketEventArgs(e);

                Disconnect();
            }
        }

        #endregion

        /// <summary>
        /// Create socket event args.
        /// </summary>
        /// <returns></returns>
        private SocketAsyncEventArgs NewSocketEventArgs()
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);

            return e;
        }

        /// <summary>
        /// Dispose socket event args.
        /// </summary>
        /// <param name="e"></param>
        private void DisposeSocketEventArgs(SocketAsyncEventArgs e)
        {
            if (e == null) return;

            try
            {
                e.Completed -= new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                e.SetBuffer(null, 0, 0);
                e.UserToken = null;
            }
            catch (Exception ex) { }
            finally
            {
                e.Dispose();
                e = null;
            }
        }
    }
}