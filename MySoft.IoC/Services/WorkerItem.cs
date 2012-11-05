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
        private Thread asyncThread;

        //响应对象
        private WaitResult waitResult;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="waitResult"></param>
        public WorkerItem(WaitResult waitResult)
        {
            this.IsCompleted = false;
            this.IsAsyncRequest = false;
            this.waitResult = waitResult;
        }

        /// <summary>
        /// 获取结果并处理超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ResponseMessage GetResult(TimeSpan timeout)
        {
            if (!waitResult.WaitOne(timeout))
            {
                //结束线程
                if (IsAsyncRequest)
                {
                    CancelThread();
                }

                throw new System.TimeoutException("Timeout occured.");
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
            return waitResult.SetResponse(resMsg);
        }

        /// <summary>
        /// 设置线程
        /// </summary>
        /// <param name="thread"></param>
        public void SetThread(Thread thread)
        {
            this.asyncThread = thread;
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        public bool Cancel()
        {
            //结束线程
            if (IsAsyncRequest)
            {
                CancelThread();
            }

            return Set(null);
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        private void CancelThread()
        {
            //结束线程
            if (asyncThread != null)
            {
                try
                {
                    asyncThread.Abort();
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
            this.asyncThread = null;
            this.waitResult = null;
        }

        #endregion
    }
}