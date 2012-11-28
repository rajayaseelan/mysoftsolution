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
        /// 完成状态
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// 上下文对象
        /// </summary>
        public OperationContext Context { get { return context; } }

        /// <summary>
        /// 请求对象
        /// </summary>
        public RequestMessage Request { get { return reqMsg; } }

        //响应对象
        private OperationContext context;
        private RequestMessage reqMsg;
        private WaitCallback callback;
        private WaitResult waitResult;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(WaitCallback callback, OperationContext context, RequestMessage reqMsg)
        {
            this.IsCompleted = false;

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
            ThreadPool.QueueUserWorkItem(callback, this);

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