using System;
using System.IO;
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
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int BufferSize = 4 * 1024; //4KB

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private readonly TcpClient _tcpClient;

        private readonly NetworkStream _netStream;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="tcpClient">A connected Socket object that is
        /// used to communicate over network</param>
        public TcpCommunicationChannel(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _tcpClient.NoDelay = true;

            _tcpClient.SendBufferSize = BufferSize;
            _tcpClient.ReceiveBufferSize = BufferSize;

            _netStream = _tcpClient.GetStream();

            var ipEndPoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
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

            _running = false;

            try
            {
                if (_tcpClient.Connected)
                {
                    _tcpClient.Close();

                    _netStream.Close();
                    _netStream.Dispose();
                }
            }
            catch
            {

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

            try
            {
                _netStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReceiveCallback), null);
            }
            catch (IOException)
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                OnMessageError(ex);
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
                throw new SocketException((int)SocketError.ConnectionReset);
            }

            try
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                //Start from threadPool
                ManagedThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        //Send all bytes to the remote application
                        _netStream.BeginWrite(messageBytes, 0, messageBytes.Length, new AsyncCallback(SendCallback), message);
                    }
                    catch (IOException)
                    {
                        Disconnect();
                    }
                    catch (Exception ex)
                    {
                        OnMessageError(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                OnMessageError(ex);

                throw ex;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                _netStream.EndWrite(ar);

                //Record last sent time
                LastSentMessageTime = DateTime.Now;

                //Sent success
                OnMessageSent(ar.AsyncState as IScsMessage);
            }
            catch (IOException)
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                OnMessageError(ex);
            }
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!_running)
            {
                return;
            }

            try
            {
                //Get received bytes count
                var bytesRead = _netStream.EndRead(ar);

                if (bytesRead > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesRead];
                    Array.Copy(_buffer, 0, receivedBytes, 0, bytesRead);

                    //Read messages according to current wire protocol
                    var messages = WireProtocol.CreateMessages(receivedBytes);

                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        OnMessageReceived(message);
                    }
                }
                else
                {
                    Disconnect();

                    return;
                }

                //Read more bytes if still running
                if (_running)
                {
                    _netStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReceiveCallback), null);
                }
            }
            catch (IOException)
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                OnMessageError(ex);
            }
        }

        #endregion
    }
}