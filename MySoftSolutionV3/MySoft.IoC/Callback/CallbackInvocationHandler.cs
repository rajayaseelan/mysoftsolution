using System;
using System.Net.Sockets;
using MySoft.Communication.Scs.Communication;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler
    {
        private Type callType;
        private IScsServerClient client;
        private int timeout = ServiceConfig.DEFAULT_SERVER_TIMEOUT;
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
        public object Invoke(object proxy, System.Reflection.MethodInfo method, object[] parameters)
        {
            if (client.CommunicationState == CommunicationStates.Connected)
            {
                var message = new CallbackMessage { ServiceType = callType, MethodName = method.ToString(), Parameters = parameters };

                //发送i回调消息
                var caller = new AsyncSendMessage((c, m) => c.SendMessage(m));

                //异步调用
                var ar = caller.BeginInvoke(client, new ScsCallbackMessage(message), callback => { }, caller);
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout)))
                {
                    //发送超时
                    throw new SocketException((int)SocketError.TimedOut);
                }

                //关闭
                ar.AsyncWaitHandle.Close();

                //释放资源
                caller.EndInvoke(ar);

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
