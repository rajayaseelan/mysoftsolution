using System;
using System.Collections.Generic;
using System.Threading;
using MySoft.Communication.Scs.Server;
using MySoft.IoC.Messages;
using System.Diagnostics;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务调用者
    /// </summary>
    public class ServiceCaller : IDisposable
    {
        private IServiceContainer container;
        private IDictionary<string, Type> callbackTypes;

        /// <summary>
        /// 初始化ServiceCaller
        /// </summary>
        /// <param name="container"></param>
        /// <param name="callbackTypes"></param>
        public ServiceCaller(IServiceContainer container, IDictionary<string, Type> callbackTypes)
        {
            this.container = container;
            this.callbackTypes = callbackTypes;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <returns></returns>
        public ResponseMessage CallMethod(IScsServerClient client, RequestMessage reqMsg, AppCaller caller, out long elapsedMilliseconds)
        {
            Thread thread = null;

            //生成一个异步调用委托
            var asyncCaller = new AsyncMethodCaller<ResponseMessage, RequestMessage>(state =>
            {
                thread = Thread.CurrentThread;

                //实例化当前上下文
                Type callbackType = null;
                if (callbackTypes.ContainsKey(reqMsg.ServiceName)) callbackType = callbackTypes[reqMsg.ServiceName];
                OperationContext.Current = new OperationContext(client, callbackType) { Caller = caller };

                return container.CallService(reqMsg);
            });

            //启动计时
            var watch = Stopwatch.StartNew();

            //开始调用
            IAsyncResult ar = asyncCaller.BeginInvoke(reqMsg, iar => { }, asyncCaller);

            var elapsedTime = TimeSpan.FromSeconds(reqMsg.Timeout);

            //等待信号，等待5分钟
            bool timeout = !ar.AsyncWaitHandle.WaitOne(elapsedTime);

            //停止计时
            watch.Stop();
            elapsedMilliseconds = watch.ElapsedMilliseconds;

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

                string body = string.Format("【{3}】Call service ({0},{1}) timeout ({2} ms)！", reqMsg.ServiceName, reqMsg.MethodName, elapsedMilliseconds, reqMsg.TransactionId);
                throw new WarningException(body)
                {
                    ApplicationName = reqMsg.AppName,
                    ServiceName = reqMsg.ServiceName,
                    ErrorHeader = string.Format("Application【{0}】call service timeout. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                };
            }

            //获取返回结果
            var resMsg = asyncCaller.EndInvoke(ar);

            //关闭句柄
            ar.AsyncWaitHandle.Close();

            return resMsg;
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.container.Dispose();
            this.callbackTypes.Clear();
        }

        #endregion
    }
}
