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
            //实例化当前上下文
            Type callbackType = null;
            if (callbackTypes.ContainsKey(reqMsg.ServiceName)) callbackType = callbackTypes[reqMsg.ServiceName];
            OperationContext.Current = new OperationContext(client, callbackType) { Caller = caller };

            //启动计时
            var watch = Stopwatch.StartNew();

            //获取返回结果
            var resMsg = container.CallService(reqMsg);

            //停止计时
            watch.Stop();

            //记录时间
            elapsedMilliseconds = watch.ElapsedMilliseconds;

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
