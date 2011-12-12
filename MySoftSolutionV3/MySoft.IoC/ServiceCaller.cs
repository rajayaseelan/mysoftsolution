using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller
    {
        private IServiceContainer container;
        private IDictionary<string, Type> callbackTypes;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="container"></param>
        /// <param name="callbackTypes"></param>
        /// <param name="logTime"></param>
        public ServiceCaller(IServiceContainer container, IDictionary<string, Type> callbackTypes)
        {
            this.container = container;
            this.callbackTypes = callbackTypes;
        }

        /// <summary>
        /// 调用 方法
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage CallMethod(IScsServerClient client, RequestMessage reqMsg)
        {
            Thread thread = null;

            //生成一个异步调用委托
            var caller = new AsyncMethodCaller<ResponseMessage, RequestMessage>(state =>
            {
                thread = Thread.CurrentThread;

                //实例化当前上下文
                Type callbackType = null;
                if (callbackTypes.ContainsKey(state.ServiceName)) callbackType = callbackTypes[state.ServiceName];
                OperationContext.Current = new OperationContext(client, callbackType);

                return container.CallService(state);
            });

            //开始调用
            IAsyncResult ar = caller.BeginInvoke(reqMsg, iar => { }, caller);

            var elapsedTime = TimeSpan.FromSeconds(reqMsg.Timeout);

            //等待信号，等待5分钟
            bool timeout = !ar.AsyncWaitHandle.WaitOne(elapsedTime);

            if (timeout)
            {
                try
                {
                    if (!ar.IsCompleted && thread != null)
                        thread.Abort();
                }
                catch (Exception ex)
                {
                }

                string title = string.Format("Call local service ({0},{1}) timeout.", reqMsg.ServiceName, reqMsg.MethodName);
                string body = string.Format("【{3}】Call service ({0},{1}) timeout ({2} ms)！", reqMsg.ServiceName, reqMsg.MethodName, elapsedTime.TotalMilliseconds, reqMsg.TransactionId);
                throw new WarningException(body)
                {
                    ApplicationName = reqMsg.AppName,
                    ServiceName = reqMsg.ServiceName,
                    ExceptionHeader = string.Format("Application【{0}】call service timeout. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                };
            }

            //获取返回结果
            var resMsg = caller.EndInvoke(ar);

            //关闭句柄
            ar.AsyncWaitHandle.Close();

            return resMsg;
        }
    }
}
