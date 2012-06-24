using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Threading;
using MySoft.IoC.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler
    {
        private Type callType;

        private IScsServerClient client;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        public CallbackInvocationHandler(Type callType, IScsServerClient client)
        {
            this.callType = callType;
            this.client = client;

            this._syncLock = new object();
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
                lock (_syncLock)
                {
                    //定义回调的消息
                    var message = new CallbackMessage
                    {
                        ServiceName = callType.FullName,
                        MethodName = method.ToString(),
                        Parameters = parameters
                    };

                    //回发消息
                    IScsMessage scsMessage = new ScsCallbackMessage(message);

                    //发送回调数据
                    client.SendMessage(scsMessage);

                    //返回null
                    return null;
                }
            }
            else
            {
                throw new SocketException((int)SocketError.ConnectionAborted);
            }
        }

        #endregion
    }
}
