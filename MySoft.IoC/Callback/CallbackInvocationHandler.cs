using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC.Callback
{
    /// <summary>
    /// 回调代理
    /// </summary>
    internal class CallbackInvocationHandler : IProxyInvocationHandler
    {
        private Type _callType;
        private IScsServerClient _channel;

        public CallbackInvocationHandler(Type callType, IScsServerClient channel)
        {
            this._callType = callType;
            this._channel = channel;
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

            //发送回调数据
            _channel.SendMessage(new ScsCallbackMessage(message));

            return null;
        }

        #endregion
    }
}
