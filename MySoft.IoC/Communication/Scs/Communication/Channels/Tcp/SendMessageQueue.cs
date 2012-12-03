using System;
using System.Collections.Generic;
using System.Net.Sockets;
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

        private bool _isCompleted;
        private Queue<MessageUserToken> _msgQueue = new Queue<MessageUserToken>();

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        /// <summary>
        /// 实例化ScsMessageQueue
        /// </summary>
        /// <param name="clientSocket"></param>
        public SendMessageQueue(Socket clientSocket)
        {
            this._clientSocket = clientSocket;
            this._syncLock = new object();
            this._isCompleted = true;
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <param name="messageBytes"></param>
        public void Send(SocketAsyncEventArgs e, IScsMessage message, byte[] messageBytes)
        {
            //实例化MessageUserToken
            var msg = new MessageUserToken(message, messageBytes);

            lock (_syncLock)
            {
                if (_isCompleted)
                {
                    _isCompleted = false;
                    SendAsync(e, msg);
                }
                else
                {
                    _msgQueue.Enqueue(msg);
                }
            }
        }

        /// <summary>
        /// 重发消息
        /// </summary>
        /// <param name="e"></param>
        public void Resend(SocketAsyncEventArgs e)
        {
            lock (_syncLock)
            {
                if (_msgQueue.Count == 0)
                {
                    _isCompleted = true;
                }
                else
                {
                    var message = _msgQueue.Dequeue();
                    SendAsync(e, message);
                }
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
                if (message == null) return;

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
                lock (_syncLock)
                {
                    while (_msgQueue.Count > 0)
                    {
                        var message = _msgQueue.Dequeue();
                        message.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_syncLock)
                {
                    _msgQueue.Clear();
                }
            }
            finally
            {
                _msgQueue = null;
            }
        }

        #endregion
    }
}
