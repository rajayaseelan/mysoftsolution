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
        //响应对象
        private OperationContext context;
        private RequestMessage reqMsg;
        private WaitResult waitResult;
        private Thread currentThread;
        private bool isCompleted;

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
        /// 当前线程
        /// </summary>
        public Thread CurrentThread
        {
            get { return currentThread; }
            set { currentThread = value; }
        }

        /// <summary>
        /// 是否结束
        /// </summary>
        public bool IsCompleted
        {
            get { return isCompleted; }
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

            this.isCompleted = false;
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
                isCompleted = true;

                //结束线程
                AbortThread(currentThread);

                //超时异常信息
                return GetTimeoutResponse(reqMsg, timeout);
            }

            return waitResult.Message;
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="thread"></param>
        private void AbortThread(Thread thread)
        {
            if (thread != null)
            {
                try
                {
                    thread.Abort();
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            if (isCompleted || resMsg == null)
                return false;
            else
                return waitResult.Set(resMsg);
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

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            context.Dispose();
            waitResult.Dispose();

            context = null;
            reqMsg = null;
            waitResult = null;
        }

        #endregion
    }
}