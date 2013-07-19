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
        /// <param name="type"></param>
        public AsyncCaller(IService service, ServiceCacheType type)
        {
            this.handler = new AsyncHandler(service, type);
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
            using (var waitResult = new WaitResult())
            {
                //Invoke响应
                handler.BeginDoTask(context, reqMsg, AsyncCallback, waitResult);

                //超时返回
                if (!waitResult.WaitOne(timeout))
                {
                    //获取超时异常
                    var ex = GetTimeoutException(reqMsg, timeout);

                    //设置超时响应
                    waitResult.Set(IoCHelper.GetResponse(reqMsg, ex));
                }

                return waitResult.Message;
            }
        }

        /// <summary>
        /// 异步回调
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            try
            {
                var waitResult = ar.AsyncState as WaitResult;

                var resMsg = handler.EndDoTask(ar);

                waitResult.Set(resMsg);
            }
            catch (Exception ex)
            {
                //TODO
            }
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
        /// 获取系统异常响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private Exception GetTimeoutException(RequestMessage reqMsg, TimeSpan timeout)
        {
            var title = string.Format("Server async invoke method ({0}, {1}) timeout ({2}) ms.\r\nParameters => {3}",
                                        reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds, reqMsg.Parameters.ToString());

            //获取异常
            throw new TimeoutException(title);
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