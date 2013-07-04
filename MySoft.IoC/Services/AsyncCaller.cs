using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : IDisposable
    {
        private AsyncHandler handler;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        public AsyncCaller(IService service)
        {
            this.handler = new AsyncHandler(service);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ResponseMessage AsyncRun(OperationContext context, RequestMessage reqMsg, TimeSpan timeout)
        {
            //Invoke响应
            var ar = handler.BeginDoTask(context, reqMsg, null, null);

            //超时返回
            if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
            {
                return GetTimeoutResponse(reqMsg, timeout);
            }

            return handler.EndDoTask(ar);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage SyncRun(OperationContext context, RequestMessage reqMsg)
        {
            //同步响应
            return handler.DoTask(context, reqMsg);
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg, TimeSpan timeout)
        {
            var title = string.Format("Async invoke method ({0}, {1}) timeout ({2}) ms.", reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            //获取异常
            return IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.handler = null;
        }
    }
}