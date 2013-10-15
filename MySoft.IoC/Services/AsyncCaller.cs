using MySoft.IoC.Messages;
using System;
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
        /// <param name="timeout"></param>
        public AsyncCaller(IService service, TimeSpan timeout)
            : base(service)
        {
            this.pool = new TaskPool(Environment.ProcessorCount, 1, 2);
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

                //获取队列管理
                var manager = GetManager(context.Caller, waitResult, out isRunTask);

                if (isRunTask)
                {
                    //开始一个异步任务
                    var _context = new AsyncContext
                    {
                        Manager = manager,
                        Context = context,
                        Request = reqMsg
                    };

                    pool.AddTaskItem(WorkCallback, _context);
                }

                if (!waitResult.WaitOne(timeout))
                {
                    //设置为结束
                    waitResult.Cancel();

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

            //解析上下文
            var ac = state as AsyncContext;

            try
            {
                //调用服务
                var resMsg = AsyncRun(ac.Manager, ac.Context, ac.Request);

                ac.Manager.Set(resMsg);
            }
            catch (Exception ex)
            {
                ac.Manager.Set(ex);
            }
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage AsyncRun(QueueManager manager, OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                //调用基类的方法
                var resMsg = base.Invoke(context, reqMsg);

                //处理符合条件的数据，大于100时也进行数据压缩
                if (manager.Count > 1)
                {
                    resMsg = new ResponseBuffer
                    {
                        ServiceName = resMsg.ServiceName,
                        MethodName = resMsg.MethodName,
                        Parameters = resMsg.Parameters,
                        ElapsedTime = resMsg.ElapsedTime,
                        Error = resMsg.Error,
                        Buffer = IoCHelper.SerializeObject(resMsg.Value)
                    };
                }

                return resMsg;
            }
            finally
            {
                //移除指定的值
                lock (queues)
                {
                    queues.Remove(manager.Key);
                }
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
        /// 清理资源
        /// </summary>
        public override void Dispose()
        {
            this.queues.Clear();
            this.pool.Dispose();
        }
    }
}