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
        private OperationContext context;
        private RequestMessage reqMsg;
        private AsyncCallback callback;
        private WaitResult waitResult;
        private AsyncWorker caller;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="callback"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(AsyncWorker caller, AsyncCallback callback, OperationContext context, RequestMessage reqMsg)
        {
            this.IsCompleted = false;

            this.caller = caller;
            this.callback = callback;
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
            //开始异步请求
            caller.BeginInvoke(context, reqMsg, callback, this);

            if (!waitResult.WaitOne(timeout))
            {
                throw new TimeoutException("Timeout occured.");
            }

            return waitResult.Message;
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool SetResult(ResponseMessage resMsg)
        {
            this.IsCompleted = true;

            if (waitResult == null)
                return false;

            return waitResult.Set(resMsg);
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        public bool Cancel()
        {
            return SetResult(null);
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            this.context.Dispose();
            this.waitResult.Dispose();

            this.context = null;
            this.reqMsg = null;
            this.callback = null;
            this.waitResult = null;
        }

        #endregion
    }
}