using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        private Semaphore semaphore;
        private bool isAsync;
        private TimeSpan timeout;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="isAsync"></param>
        /// <param name="maxCaller"></param>
        public AsyncCaller(bool isAsync, int maxCaller)
        {
            this.isAsync = isAsync;
            this.semaphore = new Semaphore(maxCaller, maxCaller);
            this.timeout = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_SERVER_TIMEOUT);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseItem Run(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //请求一个控制器
            semaphore.WaitOne();

            try
            {
                //异步处理器
                var handler = new AsyncHandler(service, context, reqMsg);

                //同步响应
                if (isAsync)
                {
                    //Invoke响应
                    var ar = handler.BeginDoTask(null, null);

                    //超时返回
                    if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
                    {
                        return GetTimeoutResponse(reqMsg);
                    }

                    return handler.EndDoTask(ar);
                }
                else
                {
                    return handler.DoTask();
                }
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
        /// <returns></returns>
        private ResponseItem GetTimeoutResponse(RequestMessage reqMsg)
        {
            var title = string.Format("Remote invoke service ({0}, {1}) timeout ({2}) ms.", reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            //获取异常
            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));

            return new ResponseItem(resMsg);
        }
    }
}