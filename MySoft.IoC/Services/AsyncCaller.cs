using MySoft.IoC.Messages;
using MySoft.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用委托
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncMethodCaller(AsyncContext context);

    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller : SyncCaller
    {
        private TimeSpan timeout;
        private AsyncMethodCaller caller;
        private IDictionary<string, QueueManager> queues;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        public AsyncCaller(IService service, TimeSpan timeout)
            : base(service)
        {
            this.caller = new AsyncMethodCaller(AsyncRun);

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

                    //开始异步调用
                    caller.BeginInvoke(_context, AsyncCallback, manager);
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
        /// 异步调用
        /// </summary>
        /// <param name="ac"></param>
        /// <returns></returns>
        private ResponseMessage AsyncRun(AsyncContext ac)
        {
            try
            {
                //调用基类服务
                var resMsg = base.Invoke(ac.Context, ac.Request);

                //处理符合条件的数据
                if (ac.Manager.Count > 1 && !(resMsg is ResponseBuffer))
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
                    queues.Remove(ac.Manager.Key);
                }
            }
        }

        /// <summary>
        /// 回调处理
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            if (ar.AsyncState == null) return;

            //处理队列管理器
            var manager = ar.AsyncState as QueueManager;

            try
            {
                //调用服务
                var resMsg = caller.EndInvoke(ar);

                manager.Set(resMsg);
            }
            catch (Exception ex)
            {
                manager.Set(ex);
            }
            finally
            {
                //关闭句柄
                ar.AsyncWaitHandle.Close();
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
            }

            //返回队列服务
            return queues[queueKey];
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

            this.caller = null;
        }
    }
}