using System;
using System.Reflection;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;
using MySoft.Communication.Scs.Communication;
using System.Net.Sockets;

namespace MySoft.IoC
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler
    {
        private Type callType;
        private IScsServerClient client;
        public CallbackInvocationHandler(Type callType, IScsServerClient client)
        {
            this.callType = callType;
            this.client = client;
        }

        #region IProxyInvocationHandler 成员

        /// <summary>
        /// 响应消息
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(object proxy, MethodInfo method, object[] parameters)
        {
            if (client.CommunicationState == CommunicationStates.Connected)
            {
                var value = new CallbackMessage { ServiceType = callType, MethodName = method.ToString(), Parameters = parameters };
                client.SendMessage(new ScsCallbackMessage(value));
                return null;
            }
            else
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
        }

        #endregion
    }
}
