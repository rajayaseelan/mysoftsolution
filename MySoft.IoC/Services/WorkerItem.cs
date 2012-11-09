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
            this.waitResult = waitResult;
        }

        /// <summary>
        /// 获取结果并处理超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ResponseMessage GetResult(TimeSpan timeout, WaitCallback callback)
        {
            //开始异步请求
            ThreadPool.QueueUserWorkItem(callback, this);

            if (!waitResult.WaitOne(timeout))
            {
                throw new System.TimeoutException("Timeout occured.");
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
            AbortThread();

            return SetResult(null);
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        private void AbortThread()
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