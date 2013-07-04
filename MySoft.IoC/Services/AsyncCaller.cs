using MySoft.Cache;
using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        private LocalCacheType cacheType;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="cacheType"></param>
        public AsyncCaller(LocalCacheType cacheType)
        {
            this.cacheType = cacheType;
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ResponseMessage AsyncRun(IService service, OperationContext context, RequestMessage reqMsg, TimeSpan timeout)
        {
            try
            {
                //异步处理器
                var handler = new AsyncHandler(service, cacheType);

                //Invoke响应
                var ar = handler.BeginDoTask(context, reqMsg, null, null);

                //超时返回
                if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    return GetTimeoutResponse(reqMsg, timeout);
                }

                return handler.EndDoTask(ar);
            }
            catch (Exception ex)
            {
                //获取异常响应
                return IoCHelper.GetResponse(reqMsg, ex);
            }
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage SyncRun(IService service, OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                //异步处理器
                var handler = new AsyncHandler(service, cacheType);

                //同步响应
                return handler.DoTask(context, reqMsg);
            }
            catch (Exception ex)
            {
                //获取异常响应
                return IoCHelper.GetResponse(reqMsg, ex);
            }
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
    }
}