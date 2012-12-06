using System;
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
        private AsyncMethodCaller workCaller;
        private WaitResult waitResult;
        private bool isCompleted;

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
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(AsyncMethodCaller caller, OperationContext context, RequestMessage reqMsg)
        {
            this.workCaller = caller;
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
            workCaller.BeginInvoke(context, reqMsg, AsyncCallback, this);

            //等待响应
            if (!waitResult.WaitOne(timeout))
            {
                isCompleted = true;

                //超时异常信息
                return GetTimeoutResponse(reqMsg, timeout);
            }

            return waitResult.Message;
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public bool Set(IAsyncResult ar)
        {
            try
            {
                var resMsg = workCaller.EndInvoke(ar);

                if (isCompleted || resMsg == null)
                    return false;
                else
                    return waitResult.Set(resMsg);
            }
            finally
            {
                workCaller = null;
            }
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var item = ar.AsyncState as WorkerItem;

            try
            {
                //设置响应信息
                item.Set(ar);
            }
            catch (Exception ex) { }
            finally
            {
                ar.AsyncWaitHandle.Close();
            }
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

            waitResult = null;
        }

        #endregion
    }
}