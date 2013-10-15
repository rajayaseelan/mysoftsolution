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
        private Func<IScsMessage, bool> _func;

        public CallbackInvocationHandler(Type callType, IScsServerClient channel)
        {
            this._callType = callType;
            this._channel = channel;
            this._func = new Func<IScsMessage, bool>(Send);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        private bool Send(IScsMessage message)
        {
            try
            {
                if (message != null)
                {
                    //发送回调数据
                    _channel.SendMessage(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 异步回调
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        private void AsyncCallback(IAsyncResult ar)
        {
            try
            {
                var completed = _func.EndInvoke(ar);

                if (!completed)
                {
                    _channel.Disconnect();
                }
            }
            catch (Exception ex) { }
            finally
            {
                ar.AsyncWaitHandle.Close();
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

            //开始异步调用
            _func.BeginInvoke(scsMessage, AsyncCallback, null);

            //返回null
            return null;
        }

        #endregion
    }
}
