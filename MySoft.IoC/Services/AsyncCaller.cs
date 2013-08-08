using Amib.Threading;
using MySoft.IoC.Messages;
using System;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private IWorkItemsGroup group;
        private TimeSpan timeout;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="group"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        public AsyncCaller(IWorkItemsGroup group, IService service, TimeSpan timeout)
            : base(service)
        {
            this.group = group;
            this.group.Start();
            this.timeout = timeout;
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public override ResponseMessage Invoke(OperationContext context, RequestMessage reqMsg)
        {
            //开始异步任务
            var worker = group.QueueWorkItem<OperationContext, RequestMessage, ResponseMessage>(base.Invoke, context, reqMsg);

            if (!group.WaitForIdle(timeout))
            {
                worker.Cancel(true);

                throw new TimeoutException(string.Format("The current request timeout {0} ms!", timeout.TotalMilliseconds));
            }

            //结果
            return worker.Result;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this.group.Cancel(true);
            }
            catch (Exception ex)
            {
            }
        }
    }
}