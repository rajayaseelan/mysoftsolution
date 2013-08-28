using MySoft.IoC.Messages;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private TimeSpan timeout;
        private TaskPool pool;
        private IDictionary<string, QueueManager> queues;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="concurrency"></param>
        /// <param name="timeout"></param>
        public AsyncCaller(IService service, int concurrency, TimeSpan timeout)
            : base(service)
        {
            this.pool = new TaskPool(concurrency, 0, 2);
            this.queues = new Dictionary<string, QueueManager>();
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
                bool isRunTask;

                var manager = GetManager(context.Caller, waitResult, out isRunTask);

                if (isRunTask)
                {
                    //开始一个异步任务
                    pool.AddTaskItem(WorkCallback, new ArrayList { manager, context, reqMsg });
                }

                if (!waitResult.WaitOne(timeout))
                {
                    throw new TimeoutException(string.Format("The current request timeout {0} ms!", timeout.TotalMilliseconds));
                }

                return waitResult.Message;
            }
        }

        /// <summary>
        /// 是否运行任务
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="waitResult"></param>
        /// <param name="isRunTask"></param>
        /// <returns></returns>
        private QueueManager GetManager(AppCaller caller, WaitResult waitResult, out bool isRunTask)
        {
            //获取queueKey值
            var queueKey = string.Format("{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);

            lock (queues)
            {
                if (!queues.ContainsKey(queueKey))
                {
                    queues[queueKey] = new QueueManager(queueKey);
                }

                //判断是否运行任务
                isRunTask = queues[queueKey].Count == 0;

                //添加到队列
                queues[queueKey].Add(waitResult);

                //返回队列服务
                return queues[queueKey];
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

            //解析数据
            var manager = arr[0] as QueueManager;
            if (manager == null) return;

            try
            {
                var context = arr[1] as OperationContext;
                var reqMsg = arr[2] as RequestMessage;

                //获取响应信息
                var resMsg = base.Invoke(context, reqMsg);

                manager.Set(resMsg);
            }
            catch (Exception ex)
            {
                //设置异常
                manager.Set(ex);
            }
            finally
            {
                lock (queues)
                {
                    //移除对应的Key
                    queues.Remove(manager.Key);
                }
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Dispose()
        {
            queues.Clear();
            pool.Dispose();
        }
    }
}