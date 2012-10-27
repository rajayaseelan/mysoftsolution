using System;
using MySoft.IoC.Messages;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// Worker item.
    /// </summary>
    internal class WorkerItem
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
        /// Thread
        /// </summary>
        public Thread Thread { get; set; }

        //响应对象
        private WaitResult waitResult;

        /// <summary>
        /// 实例化WorkerItem
        /// </summary>
        /// <param name="waitResult"></param>
        public WorkerItem(WaitResult waitResult)
        {
            this.waitResult = waitResult;
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        public bool Set(ResponseMessage resMsg)
        {
            return waitResult.SetResponse(resMsg);
        }
    }
}
