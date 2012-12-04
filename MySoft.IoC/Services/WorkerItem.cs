using System;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Worker item.
    /// </summary>
    internal class WorkerItem : IDisposable
    {
        /// <summary>
        /// 完成状态
        /// </summary>
        public bool IsCompleted { get; private set; }

        //响应对象
        private readonly AsyncMethodCaller caller;
        private readonly OperationContext context;
        private readonly RequestMessage reqMsg;
        private WaitResult waitResult;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(AsyncMethodCaller caller, OperationContext context, RequestMessage reqMsg)
        {
            this.IsCompleted = false;

            this.caller = caller;
            this.context = context;
            this.reqMsg = reqMsg;
            this.waitResult = new WaitResult(reqMsg);
        }

        /// <summary>
        /// 获取结果并处理超时
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ResponseMessage GetResult(AsyncCallback callback, TimeSpan timeout)
        {
            //开始异步请求
            caller.BeginInvoke(context, reqMsg, callback, this);

            if (!waitResult.WaitOne(timeout))
            {
                this.IsCompleted = true;

                //超时异常信息
                return GetTimeoutResponse(reqMsg, timeout);
            }

            return waitResult.Message;
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            this.IsCompleted = true;

            return waitResult.Set(resMsg);
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            this.context.Dispose();

            this.waitResult.Dispose();
            this.waitResult = null;
        }

        #endregion

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
    }
}