using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using MySoft.Communication.Scs.Communication;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Threading;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler
    {
        private static Queue queue = Queue.Synchronized(new Queue());

        private Type callType;
        private IScsServerClient client;
        public CallbackInvocationHandler(Type callType, IScsServerClient client)
        {
            this.callType = callType;
            this.client = client;

            //启用线程进行数据推送
            ManagedThreadPool.QueueUserWorkItem(DoSend);
        }

        private void DoSend(object state)
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    CallbackInfo info = null;
                    lock (queue.SyncRoot)
                    {
                        var message = queue.Dequeue();
                        info = message as CallbackInfo;
                    }

                    //发送消息
                    if (info != null)
                    {
                        try
                        {
                            var client = info.Client;
                            var message = new ScsCallbackMessage(info.Message);

                            //发送回调数据
                            client.SendMessage(message);
                        }
                        catch (Exception ex)
                        {
                            //TO DO
                        }
                    }
                }

                //等待10毫秒
                Thread.Sleep(10);
            }
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
            if (client.CommunicationState == CommunicationStates.Connected)
            {
                //定义回调的消息
                var message = new CallbackMessage
                {
                    ServiceName = callType.FullName,
                    MethodName = method.ToString(),
                    Parameters = parameters
                };

                //加入队列
                lock (queue.SyncRoot)
                {
                    queue.Enqueue(new CallbackInfo
                    {
                        Client = client,
                        Message = message
                    });
                }

                //返回null
                return null;
            }
            else
            {
                throw new SocketException((int)SocketError.ConnectionAborted);
            }
        }

        #endregion
    }
}
