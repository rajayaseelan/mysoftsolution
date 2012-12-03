using System;
using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// Send message queue.
    /// </summary>
    internal class SendMessageQueue : IDisposable
    {
        /// <summary>
        /// 用于完成异步操作的事件
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> Completed;

        private readonly Socket _clientSocket;
        private readonly SocketAsyncEventArgs _sendEventArgs;
        private ManualResetEvent _manualResetEvent;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        /// <summary>
        /// 实例化ScsMessageQueue
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="sendEventArgs"></param>
        public SendMessageQueue(Socket clientSocket, SocketAsyncEventArgs sendEventArgs)
        {
            this._clientSocket = clientSocket;
            this._sendEventArgs = sendEventArgs;
            this._syncLock = new object();
            this._manualResetEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageBytes"></param>
        public void Send(IScsMessage message, byte[] messageBytes)
        {
            //实例化MessageUserToken
            var msg = new MessageUserToken(message, messageBytes);

            lock (_syncLock)
            {
                SendAsync(_sendEventArgs, msg);

                try
                {
                    if (_manualResetEvent.WaitOne())
                    {
                        _manualResetEvent.Reset();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 重置消息
        /// </summary>
        /// <param name="e"></param>
        public void Reset(SocketAsyncEventArgs e)
        {
            try
            {
                e.SetBuffer(null, 0, 0);
                e.UserToken = null;

                _manualResetEvent.Set();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        private void SendAsync(SocketAsyncEventArgs e, MessageUserToken message)
        {
            try
            {
                if (e == null) return;

                e.UserToken = message.Message;

                //设置缓冲区
                e.SetBuffer(message.Buffer, 0, message.Buffer.Length);

                //开始异步发送
                if (!_clientSocket.SendAsync(e))
                {
                    if (Completed != null)
                    {
                        Completed(_clientSocket, e);
                    }
                }
            }
            finally
            {
                message.Dispose();
                message = null;
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose resource.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _manualResetEvent.Close();
            }
            catch (Exception ex) { }
            finally
            {
                _manualResetEvent = null;
            }
        }

        #endregion
    }
}
