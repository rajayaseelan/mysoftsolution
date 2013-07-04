using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using System;
using System.Collections;
using System.Threading;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler
    {
        private Queue _queue;
        private Type _callType;
        private IScsServerClient _channel;

        public CallbackInvocationHandler(Type callType, IScsServerClient channel)
        {
            this._callType = callType;
            this._channel = channel;
            this._queue = Queue.Synchronized(new Queue());

            ThreadPool.QueueUserWorkItem(DoWork);
        }

        /// <summary>
        /// 定时任务
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            while (!_channel.Canceled)
            {
                Thread.Sleep(100);

                if (_queue.Count == 0) continue;

                try
                {
                    var message = _queue.Dequeue() as IScsMessage;

                    if (message != null)
                    {
                        //发送回调数据
                        _channel.SendMessage(message);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            //清除并断开连接
            _queue.Clear();

            Thread.Sleep(TimeSpan.FromSeconds(1));
            _channel.Disconnect();
        }

        #region IProxyInvocationHandler 成员

        /// <summary>
        /// 响应消息
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, System.Reflection.MethodInfo method, object[] parameters)
        {
            if (_channel.CommunicationState != CommunicationStates.Connected)
            {
                throw new CommunicationStateException("The client has disconnected from the server.");
            }

            //定义回调的消息
            var message = new CallbackMessage
            {
                ServiceName = _callType.FullName,
                MethodName = method.ToString(),
                Parameters = parameters
            };

            //回发消息
            IScsMessage scsMessage = new ScsCallbackMessage(message);

            _queue.Enqueue(scsMessage);

            //返回null
            return null;
        }

        #endregion
    }
}
