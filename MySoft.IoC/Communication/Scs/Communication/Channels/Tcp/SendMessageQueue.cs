using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// Send message queue.
    /// </summary>
    internal class SendMessageQueue : IDisposable
    {
        private Socket _clientSocket;
        private Action<object, SocketAsyncEventArgs> _callback;
        private Queue<MessageUserToken> _msgQueue;
        private volatile bool _isCompleted = false;

        /// <summary>
        /// 实例化ScsMessageQueue
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="callback"></param>
        public SendMessageQueue(Socket clientSocket, Action<object, SocketAsyncEventArgs> callback)
        {
            this._clientSocket = clientSocket;
            this._callback = callback;
            this._msgQueue = new Queue<MessageUserToken>();
            this._isCompleted = true;
        }

        /// <summary>
        /// 队列大小
        /// </summary>
        public int Count
        {
            get
            {
                lock (_msgQueue)
                {
                    return _msgQueue.Count;
                }
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        public void SendMessage(SocketAsyncEventArgs e, MessageUserToken message)
        {
            lock (_msgQueue)
            {
                if (!_isCompleted)
                {
                    _msgQueue.Enqueue(message);
                }
                else
                {
                    _isCompleted = false;

                    SendAsync(e, message);
                }
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="e"></param>
        public void SendMessage(SocketAsyncEventArgs e)
        {
            lock (_msgQueue)
            {
                if (!_isCompleted) return;

                if (_msgQueue.Count == 0) return;

                //从队列中取出一个消息进行发送
                var message = _msgQueue.Dequeue();

                //异步发送消息
                SendAsync(e, message);
            }
        }

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        private void SendAsync(SocketAsyncEventArgs e, MessageUserToken message)
        {
            if (message == null) return;

            e.UserToken = message;

            //设置缓冲区
            e.SetBuffer(message.Buffer, 0, message.Buffer.Length);

            //开始异步发送
            if (!_clientSocket.SendAsync(e))
            {
                if (_callback != null)
                {
                    _callback(_clientSocket, e);
                }
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        /// <param name="e"></param>
        public void ResetMessage(SocketAsyncEventArgs e)
        {
            try
            {
                e.UserToken = null;
                e.SetBuffer(null, 0, 0);
            }
            catch (Exception ex)
            {
            }

            _isCompleted = true;
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose resource.
        /// </summary>
        public void Dispose()
        {
            lock (_msgQueue)
            {
                try
                {
                    while (_msgQueue.Count > 0)
                    {
                        var message = _msgQueue.Dequeue();
                        message.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _msgQueue.Clear();
                }
            }

            _clientSocket = null;
        }

        #endregion
    }
}
