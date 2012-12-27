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
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(OperationContext context, RequestMessage reqMsg)
        {
            this.context = context;
            this.reqMsg = reqMsg;
            this.waitResult = new WaitResult(reqMsg);
        }

        /// <summary>
        /// 获取结果并处理超时
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ResponseMessage GetResult(WaitCallback callback)
        {
            //开始异步请求
            ThreadPool.UnsafeQueueUserWorkItem(callback, this);

            //等待响应
            if (waitResult.WaitOne())
            {
                return waitResult.Message;
            }
            else
            {
                return default(ResponseMessage);
            }
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            return waitResult.Set(resMsg);
        }

        #region IDisposable 成员

        /// <summary>
        /// Dispose operation context.
        /// </summary>
        public void Dispose()
        {
            try
            {
                context.Dispose();
                waitResult.Dispose();
            }
            catch (Exception ex) { }
            finally
            {
                context = null;
                reqMsg = null;
                waitResult = null;
            }
        }

        #endregion
    }
}