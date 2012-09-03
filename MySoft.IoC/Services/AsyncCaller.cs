using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    internal class AsyncCaller
    {
        private ILog logger;
        private IService service;
        private TimeSpan elapsedTime;
        private IDictionary<string, Queue<WaitResult>> results;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan elapsedTime)
        {
            this.logger = logger;
            this.service = service;
            this.elapsedTime = elapsedTime;
            this.results = new Dictionary<string, Queue<WaitResult>>();
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage AsyncCall(OperationContext context, RequestMessage reqMsg)
        {
            AppCaller caller = context.Caller;

            var callKey = string.Format("Caller_{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);

            //从缓存中获取数据
            var resMsg = CacheHelper.Get<ResponseMessage>(callKey);

            if (resMsg != null)
            {
                //刷新工作项
                ThreadManager.RefreshWorker(callKey);

                if (resMsg.Value is InvokeData)
                {
                    (resMsg.Value as InvokeData).ElapsedTime = 0;
                }

                return new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = resMsg.ReturnType,
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ElapsedTime = 0,
                    Value = resMsg.Value,
                    Error = resMsg.Error
                };
            }

            //等待响应消息
            using (var waitResult = new WaitResult(reqMsg))
            {
                lock (results)
                {
                    if (!results.ContainsKey(callKey))
                    {
                        results[callKey] = new Queue<WaitResult>();
                        ThreadPool.QueueUserWorkItem(GetResponse, new ArrayList { callKey, waitResult, context, reqMsg });
                    }
                    else
                    {
                        results[callKey].Enqueue(waitResult);
                    }
                }

                if (!waitResult.Wait(elapsedTime))
                {
                    var body = string.Format("Remote client【{0}】call service ({1},{2}) timeout ({4}) ms.\r\nParameters => {3}",
                        reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), (int)elapsedTime.TotalMilliseconds);

                    //获取异常
                    var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

                    logger.WriteError(error);

                    var title = string.Format("Call remote service ({0},{1}) timeout ({2}) ms.",
                                reqMsg.ServiceName, reqMsg.MethodName, (int)elapsedTime.TotalMilliseconds);

                    //处理异常
                    resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
                }
                else
                {
                    resMsg = waitResult.Message;
                }

                //设置响应
                SetResponse(callKey, resMsg);

                return resMsg;
            }
        }

        /// <summary>
        /// 设置响应
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="resMsg"></param>
        private void SetResponse(string callKey, ResponseMessage resMsg)
        {
            lock (results)
            {
                if (results.ContainsKey(callKey))
                {
                    var queue = results[callKey];

                    if (queue.Count > 0)
                    {
                        //输出队列信息
                        logger.WriteLog(string.Format("【Queues: {0}】{1}, {2}.", queue.Count, resMsg.ServiceName, resMsg.MethodName), LogType.Normal);

                        while (queue.Count > 0)
                        {
                            var wr = queue.Dequeue();

                            wr.Set(resMsg);
                        }
                    }

                    //移除指定的Key
                    results.Remove(callKey);
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="state"></param>
        private void GetResponse(object state)
        {
            var arr = state as ArrayList;

            var callKey = arr[0] as string;
            var wr = arr[1] as WaitResult;
            var context = arr[2] as OperationContext;
            var reqMsg = arr[3] as RequestMessage;

            var watch = Stopwatch.StartNew();

            //获取响应的消息
            var resMsg = ThreadManager.GetResponse(service, context, reqMsg);

            watch.Stop();

            wr.Set(resMsg);

            //只缓存服务端数据
            if (resMsg != null)
            {
                var timeSpan = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_RECORD_TIMEOUT);

                //如果符合条件，则自动缓存 【自动缓存功能】
                if (!resMsg.IsError && watch.ElapsedMilliseconds > timeSpan.TotalMilliseconds)
                {
                    CacheHelper.Insert(callKey, resMsg, ServiceConfig.DEFAULT_CACHE_TIME);

                    //Add worker item
                    var worker = new WorkerItem
                    {
                        Service = service,
                        Context = context,
                        Request = reqMsg
                    };

                    ThreadManager.AddWorker(callKey, worker);
                }
            }
        }
    }
}
