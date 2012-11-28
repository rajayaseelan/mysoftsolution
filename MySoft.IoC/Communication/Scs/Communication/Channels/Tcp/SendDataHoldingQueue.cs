using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// Send DataHolding queue.
    /// </summary>
    internal class SendDataHoldingQueue
    {
        private Queue<DataHoldingUserToken> tokenQueue;

        private Action<IScsMessage> callback;
        private Func<IScsMessage, byte[]> func;
        private int bufferSize;

        /// <summary>
        /// 实例化ScsMessageQueue
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="func"></param>
        /// <param name="bufferSize"></param>
        public SendDataHoldingQueue(Action<IScsMessage> callback, Func<IScsMessage, byte[]> func, int bufferSize)
        {
            this.tokenQueue = new Queue<DataHoldingUserToken>();
            this.callback = callback;
            this.func = func;
            this.bufferSize = bufferSize;
        }

        /// <summary>
        /// 添加消息到队列
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public void SendMessage(IScsMessage message, SocketAsyncEventArgs e)
        {
            //Create a byte array from message according to current protocol
            var userToken = new DataHoldingUserToken(message, func(message));

            if (e.UserToken == null)
            {
                e.UserToken = userToken;
            }
            else
            {
                lock (tokenQueue)
                {
                    tokenQueue.Enqueue(userToken);
                }
            }

            SendMessage(e);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="e"></param>
        public void SendMessage(SocketAsyncEventArgs e)
        {
            //从上下文中获取消息
            var userToken = e.UserToken as DataHoldingUserToken;
            var messageBytes = userToken.GetRemainingBuffer(bufferSize);

            if (messageBytes == null)
            {
                //完成回调
                callback(userToken.Message);

                if (tokenQueue.Count == 0) return;

                lock (tokenQueue)
                {
                    //从队列中取出一个消息进行发送
                    userToken = tokenQueue.Dequeue();
                }

                if (userToken == null) return;

                messageBytes = userToken.GetRemainingBuffer(bufferSize);
                e.UserToken = userToken;
            }

            //设置缓冲区
            e.SetBuffer(messageBytes, 0, messageBytes.Length);

            //开始异步发送
            if (!e.AcceptSocket.SendAsync(e))
            {
                var te = e as TcpSocketAsyncEventArgs;
                if (te != null)
                {
                    te.Channel.IOCompleted(e);
                }
            }
        }
    }
}
