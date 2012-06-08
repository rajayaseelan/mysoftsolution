using System;
using System.Threading;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 返回值对象
    /// </summary>
    [Serializable]
    internal sealed class AsyncResult : IDisposable
    {
        private AutoResetEvent reset;
        private RequestMessage request;
        private ResponseMessage message;
        private OperationContext context;
        private Thread thread;

        /// <summary>
        /// 消息对象
        /// </summary>
        public ResponseMessage Message
        {
            get { return message; }
        }

        /// <summary>
        /// 请求对象
        /// </summary>
        public RequestMessage Request
        {
            get { return request; }
        }

        /// <summary>
        /// 上下文对象
        /// </summary>
        public OperationContext Context
        {
            get { return context; }
        }

        /// <summary>
        /// 实例化AsyncResult
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public AsyncResult(OperationContext context, RequestMessage reqMsg)
        {
            this.reset = new AutoResetEvent(false);
            this.context = context;
            this.request = reqMsg;
        }

        /// <summary>
        /// 等待响应
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool Wait(TimeSpan timeSpan)
        {
            return reset.WaitOne(timeSpan);
        }

        /// <summary>
        /// 响应信号
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            this.message = resMsg;
            return reset.Set();
        }

        /// <summary>
        /// 设置当前线程
        /// </summary>
        /// <param name="thread"></param>
        public void Set(Thread thread)
        {
            this.thread = thread;
        }

        /// <summary>
        /// 结束当前线程
        /// </summary>
        public void Cancel()
        {
            try
            {
                //中止线程
                if (thread != null) thread.Abort();
            }
            catch
            {
                //TODO
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.reset.Reset();

            this.thread = null;
            this.reset = null;
            this.request = null;
            this.message = null;
            this.context = null;

            //上下文设置为null
            OperationContext.Current = null;
        }

        #endregion
    }
}
