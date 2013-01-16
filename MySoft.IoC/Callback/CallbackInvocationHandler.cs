using System;
using Castle.DynamicProxy;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler, IInterceptor
    {
        private Type callType;
        private IScsServerClient channel;

        public CallbackInvocationHandler(Type callType, IScsServerClient channel)
        {
            this.callType = callType;
            this.channel = channel;
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
            channel.SendMessage(scsMessage);

            //返回null
            return null;
        }

        #endregion

        #region IInterceptor 成员

        /// <summary>
        /// Impl Intercept method.
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            //Call proxy method.
            invocation.ReturnValue = Invoke(invocation.Proxy, invocation.Method, invocation.Arguments);
        }

        #endregion
    }
}
