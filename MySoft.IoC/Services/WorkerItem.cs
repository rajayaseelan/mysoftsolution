using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Worker item.
    /// </summary>
    internal class WorkerItem : IDisposable
    {
        //响应对象
        private OperationContext context;
        private RequestMessage reqMsg;
        private WaitResult waitResult;

        /// <summary>
        /// 上下文信息
        /// </summary>
        public OperationContext Context
        {
            get { return context; }
        }

        /// <summary>
        /// 请求信息
        /// </summary>
        public RequestMessage Request
        {
            get { return reqMsg; }
        }

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(OperationContext context, RequestMessage reqMsg)
        {
            this.context = context;
            this.reqMsg = reqMsg;
            this.waitResult = new WaitResult(reqMsg);
        }

        /// <summary>
        /// 获取结果并处理超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ResponseMessage GetResult(TimeSpan timeout)
        {
            //等待响应
            if (!waitResult.WaitOne(timeout))
            {
                return GetTimeoutResponse(reqMsg, timeout);
            }

            return waitResult.Message;
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
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            return waitResult.Set(resMsg);
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            try
            {
                context.Dispose();
                waitResult.Dispose();
            }
            catch (Exception ex) { }
            finally
            {
                context = null;
                reqMsg = null;
                waitResult = null;
            }
        }

        #endregion
    }
}