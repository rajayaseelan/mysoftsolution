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
        private readonly WaitResult waitResult;
        private OperationContext context;
        private RequestMessage reqMsg;

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
        /// <param name="waitResult"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public WorkerItem(WaitResult waitResult, OperationContext context, RequestMessage reqMsg)
        {
            this.waitResult = waitResult;
            this.context = context;
            this.reqMsg = reqMsg;
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
            }
            catch (Exception ex) { }
            finally
            {
                context = null;
                reqMsg = null;
            }
        }

        #endregion
    }
}