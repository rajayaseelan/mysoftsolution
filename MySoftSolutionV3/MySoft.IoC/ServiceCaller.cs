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
        /// <param name="logTime"></param>
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
        /// <param name="elapsedMilliseconds"></param>
        /// <returns></returns>
        public ResponseMessage CallMethod(IScsServerClient client, RequestMessage reqMsg, out long elapsedMilliseconds)
        {
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(reqMsg.ServiceName)) callbackType = callbackTypes[reqMsg.ServiceName];
            OperationContext.Current = new OperationContext(client, callbackType);

            Stopwatch watch = Stopwatch.StartNew();

            //调用服务
            var resMsg = container.CallService(reqMsg);

            watch.Stop();
            elapsedMilliseconds = watch.ElapsedMilliseconds;
            var elapsedTime = TimeSpan.FromSeconds(reqMsg.Timeout);

            //计算耗时
            if (watch.ElapsedMilliseconds > elapsedTime.TotalMilliseconds)
            {
                string body = string.Format("【{3}】Call service ({0},{1}) timeout ({2} ms)！", reqMsg.ServiceName, reqMsg.MethodName, elapsedMilliseconds, reqMsg.TransactionId);
                var ex = new WarningException(body)
                {
                    ApplicationName = reqMsg.AppName,
                    ServiceName = reqMsg.ServiceName,
                    ErrorHeader = string.Format("Application【{0}】call service timeout. ==> Comes from {1}({2}).", reqMsg.AppName, reqMsg.HostName, reqMsg.IPAddress)
                };

                //如果超时，则写异常
                container.WriteError(ex);
            }

            return resMsg;
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.container.Dispose();
            this.callbackTypes = null;
        }

        #endregion
    }
}
