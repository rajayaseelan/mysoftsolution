using MySoft.IoC.Messages;
using MySoft.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private TimeSpan timeout;
        private IDictionary<string, QueueManager> queues;
        private TaskPool pool;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        public AsyncCaller(TaskPool pool, IService service, TimeSpan timeout)
            : base(service)
        {
            this.queues = new Dictionary<string, QueueManager>();
            this.timeout = timeout;
            this.pool = pool;
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

                    //开始异步调用
                    pool.AddTaskItem(AsyncCallback, _context);
                }

                if (!waitResult.WaitOne(timeout))
                {
                    //设置为结束
                    waitResult.Cancel();

                    throw new TimeoutException(string.Format("The current request timeout {0} ms!", timeout.TotalMilliseconds));
                }

                return waitResult.Response;
            }
        }

        /// <summary>
        /// 异步处理
        /// </summary>
        /// <param name="state"></param>
        private void AsyncCallback(object state)
        {
            //处理队列管理器
            var async = state as AsyncContext;
            var manager = async.Manager;

            try
            {
                //调用服务
                var request = async.Request;
                var context = async.Context;

                var resMsg = base.Invoke(context, request);

                manager.Set(resMsg);
            }
            catch (Exception ex)
            {
                manager.Set(ex);
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
            var key = string.Format("{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);
            var queueKey = MD5.HexHash(Encoding.Default.GetBytes(key));

            if (!queues.ContainsKey(queueKey))
            {
                lock (queues)
                {
                    if (!queues.ContainsKey(queueKey))
                    {
                        queues[queueKey] = new QueueManager(queueKey);
                    }
                }
            }

            //定义QueueManager
            var manager = queues[queueKey];

            //判断是否运行任务
            isRunTask = manager.Count == 0;

            //添加到队列
            manager.Add(waitResult);

            //返回队列服务
            return manager;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Dispose()
        {
            lock (queues)
            {
                this.queues.Clear();
            }
        }
    }
}