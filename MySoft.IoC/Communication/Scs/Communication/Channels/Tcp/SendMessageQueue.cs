using System.Collections.Generic;
using System.Net.Sockets;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// Send message queue.
    /// </summary>
    internal class SendMessageQueue
    {
        private Socket _clientSocket;
        private Queue<MessageUserToken> _msgQueue;
        private bool _isCompleted = false;

        /// <summary>
        /// 实例化ScsMessageQueue
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="capacity"></param>
        public SendMessageQueue(Socket clientSocket, int capacity)
        {
            this._clientSocket = clientSocket;
            this._msgQueue = new Queue<MessageUserToken>(capacity);
            this._isCompleted = true;
        }

        /// <summary>
        /// 清除队列
        /// </summary>
        public void Clear()
        {
            lock (_msgQueue)
            {
                _msgQueue.Clear();
            }
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public void SendMessage(MessageUserToken message, SocketAsyncEventArgs e)
        {
            if (_isCompleted)
            {
                _isCompleted = false;

                //发送消息
                SendAsync(e, message);
            }
            else
            {
                lock (_msgQueue)
                {
                    _msgQueue.Enqueue(message);
                }
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="e"></param>
        public void SendMessage(SocketAsyncEventArgs e)
        {
            if (_msgQueue.Count == 0) return;

            //定义消息
            MessageUserToken message = null;

            lock (_msgQueue)
            {
                //从队列中取出一个消息进行发送
                message = _msgQueue.Dequeue();
            }

            //异步发送消息
            SendAsync(e, message);
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
                var te = e as TcpSocketAsyncEventArgs;
                if (te != null)
                {
                    te.Channel.IOCompleted(e);
                }
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            _isCompleted = true;
        }
    }
}
