using System;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Threading;

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
        public OperationContext Context { get; private set; }

        /// <summary>
        /// Request
        /// </summary>
        public RequestMessage Request { get; private set; }

        /// <summary>
        /// 完成状态
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// AsyncThread
        /// </summary>
        private Thread asyncThread;

        //响应对象
        private WaitCallback callback;
        private WaitResult waitResult;
        private bool fromServer;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="fromServer"></param>
        public WorkerItem(WaitCallback callback, OperationContext context, RequestMessage reqMsg, bool fromServer)
        {
            this.IsCompleted = false;

            this.callback = callback;
            this.Context = context;
            this.Request = reqMsg;
            this.fromServer = fromServer;
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
            if (fromServer)
                ThreadPool.QueueUserWorkItem(callback, this);
            else
                ManagedThreadPool.QueueUserWorkItem(callback, this);

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

            if (waitResult == null)
                return false;

            return waitResult.Set(resMsg);
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
            this.waitResult.Dispose();

            this.Context = null;
            this.Request = null;
            this.callback = null;
            this.asyncThread = null;
            this.waitResult = null;
        }

        #endregion
    }
}