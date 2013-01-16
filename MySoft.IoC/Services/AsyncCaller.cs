using System;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private TimeSpan timeout;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, TimeSpan timeout, bool fromServer)
            : base(service, fromServer)
        {
            this.timeout = timeout;
        }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, TimeSpan timeout, IDataCache cache, bool fromServer)
            : base(service, cache, fromServer)
        {
            this.timeout = timeout;
        }

        /// <summary>
        /// 异常调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        protected override ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            //异步调用
            using (var waitResult = new WaitResult(reqMsg))
            using (var worker = new WorkerItem(waitResult, context, reqMsg))
            {
                try
                {
                    using (var afc = ExecutionContext.SuppressFlow())
                    {
                        //开始异步请求
                        ThreadPool.QueueUserWorkItem(AsyncCallback, worker);
                    }

                    //等待响应
                    if (!waitResult.WaitOne(timeout))
                    {
                        return GetTimeoutResponse(reqMsg, timeout);
                    }
                }
                catch (Exception ex)
                {
                    //处理异常响应
                    return IoCHelper.GetResponse(reqMsg, ex);
                }

                return waitResult.Message;
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
            //获取异常响应信息
            var body = string.Format("Async call service ({0}, {1}) timeout ({2}) ms.",
                        reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(body));

            //设置耗时时间
            resMsg.ElapsedTime = (long)timeout.TotalMilliseconds;

            return resMsg;
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="state"></param>
        private void AsyncCallback(object state)
        {
            var worker = state as WorkerItem;

            try
            {
                //开始同步调用
                var resMsg = base.GetResponse(worker.Context, worker.Request);

                //设置响应信息
                worker.Set(resMsg);
            }
            catch (Exception ex) { }
            finally
            {
                worker.Dispose();
            }
        }
    }
}