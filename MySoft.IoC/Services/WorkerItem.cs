using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Worker item.
    /// </summary>
    internal class WorkerItem : IDisposable
    {
        /// <summary>
        /// 调用的Key
        /// </summary>
        public string CallKey { get; set; }

        /// <summary>
        /// Context
        /// </summary>
        public OperationContext Context { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public RequestMessage Request { get; set; }

        /// <summary>
        /// 完成状态
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// 是否异步请求
        /// </summary>
        public bool IsAsyncRequest { get; set; }

        /// <summary>
        /// AsyncThread
        /// </summary>
        public Thread AsyncThread { private get; set; }

        //响应对象
        private WaitResult waitResult;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="waitResult"></param>
        public WorkerItem(WaitResult waitResult)
        {
            this.IsCompleted = false;
            this.waitResult = waitResult;
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            this.IsCompleted = true;
            return waitResult.SetResponse(resMsg);
        }

        /// <summary>
        /// 结束响应
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Cancel(TimeSpan timeout)
        {
            //结束线程
            if (IsAsyncRequest)
            {
                CancelThread();
            }

            var resMsg = GetTimeoutResponse(this.Request, timeout);

            return Set(resMsg);
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
            var title = string.Format("Async call service ({0}, {1}) timeout ({2}) ms.",
                        reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            var resMsg = IoCHelper.GetResponse(reqMsg, new System.TimeoutException(title));

            //设置耗时时间
            resMsg.ElapsedTime = (long)timeout.TotalMilliseconds;

            return resMsg;
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        private void CancelThread()
        {
            //结束线程
            if (AsyncThread != null)
            {
                try
                {
                    AsyncThread.Abort();
                }
                catch (Exception ex)
                {
                }
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            this.Context.Dispose();
            this.Context = null;
            this.Request = null;
            this.AsyncThread = null;
        }

        #endregion
    }
}
