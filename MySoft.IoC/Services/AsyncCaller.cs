using MySoft.IoC.Messages;
using System;
using System.Collections;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private TaskPool pool;
        private TimeSpan timeout;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        public AsyncCaller(IService service, TimeSpan timeout)
            : base(service)
        {
            this.pool = new TaskPool(10, 1, 2);
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
            using (var waitResult = new WaitResult(reqMsg))
            {
                //开始一个异步任务
                pool.AddTaskItem(WorkCallback, new ArrayList { waitResult, context, reqMsg });

                if (!waitResult.WaitOne(timeout))
                {
                    throw new TimeoutException(string.Format("The current request timeout {0} ms!", timeout.TotalMilliseconds));
                }

                return waitResult.Message;
            }
        }

        /// <summary>
        /// 回调处理
        /// </summary>
        /// <param name="state"></param>
        private void WorkCallback(object state)
        {
            if (state == null) return;
            var arr = state as ArrayList;

            var waitResult = arr[0] as WaitResult;

            try
            {
                var context = arr[1] as OperationContext;
                var reqMsg = arr[2] as RequestMessage;

                //调用基类方法
                var resMsg = base.Invoke(context, reqMsg);

                waitResult.Set(resMsg);
            }
            catch (Exception ex)
            {
                waitResult.Set(ex);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Dispose()
        {
            pool.Dispose();
        }
    }
}