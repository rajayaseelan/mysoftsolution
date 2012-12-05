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
        private readonly OperationContext context;
        private readonly RequestMessage reqMsg;
        private AsyncMethodCaller caller;
        private Thread currentThread;
        private WaitResult waitResult;
        private bool isCompleted;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(AsyncMethodCaller caller, OperationContext context, RequestMessage reqMsg)
        {
            this.caller = caller;
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
            //开始异步请求
            ThreadPool.QueueUserWorkItem(WaitCallback);

            //等待响应
            if (!waitResult.WaitOne(timeout))
            {
                isCompleted = true;

                //结束线程
                AbortThread();

                //超时异常信息
                return GetTimeoutResponse(reqMsg, timeout);
            }

            return waitResult.Message;
        }

        private void AbortThread()
        {
            try
            {
                if (currentThread != null)
                {
                    //结束线程
                    var state = GetThreadState(currentThread.ThreadState);

                    switch (state)
                    {
                        case ThreadState.WaitSleepJoin:
                            currentThread.Interrupt();
                            break;
                        case ThreadState.Running:
                            currentThread.Abort();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 线程状态
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private ThreadState GetThreadState(ThreadState ts)
        {
            return ts & (ThreadState.Aborted | ThreadState.AbortRequested |
                         ThreadState.Stopped | ThreadState.Unstarted |
                         ThreadState.WaitSleepJoin);
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            currentThread = Thread.CurrentThread;

            try
            {
                //获取响应信息
                var resMsg = caller.Invoke(context, reqMsg);

                //完成直接退出
                if (isCompleted) return;

                isCompleted = true;

                waitResult.Set(resMsg);
            }
            catch (Exception ex)
            {
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            context.Dispose();
            waitResult.Dispose();

            waitResult = null;
            caller = null;
            currentThread = null;
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