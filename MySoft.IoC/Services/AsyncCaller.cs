using MySoft.IoC.Messages;
using System;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        private bool serverCache;
        private Semaphore semaphore;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="maxCaller"></param>
        /// <param name="serverCache"></param>
        public AsyncCaller(int maxCaller, bool serverCache)
        {
            this.semaphore = new Semaphore(maxCaller, maxCaller);
            this.serverCache = serverCache;
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
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                //异步处理器
                var handler = new AsyncHandler(service, context, reqMsg, serverCache);

                //Invoke响应
                var ar = handler.BeginDoTask(null, null);

                //超时返回
                if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    return GetTimeoutResponse(reqMsg, timeout);
                }

                return handler.EndDoTask(ar);
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
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
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                //异步处理器
                var handler = new AsyncHandler(service, context, reqMsg, serverCache);

                //同步响应
                return handler.DoTask();
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
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
            var title = string.Format("Async invoke service ({0}, {1}) timeout ({2}) ms.", reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            //获取异常
            return IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
        }
    }
}