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
        private bool enableCache;
        private IDictionary<string, Queue<WaitResult>> results;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="enableCache"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan elapsedTime, bool enableCache)
        {
            this.logger = logger;
            this.service = service;
            this.elapsedTime = elapsedTime;
            this.enableCache = enableCache;
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
            //如果是状态服务，则同步响应
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
            {
                return ThreadManager.GetResponse(service, context, reqMsg);
            }

            //定义一个响应值
            ResponseMessage resMsg = null;

            var callKey = GetServiceCallKey(context.Caller);

            if (enableCache)
            {
                //从缓存中获取数据
                if (GetResponseFromCache(callKey, reqMsg, out resMsg))
                {
                    return resMsg;
                }
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
        /// 获取缓存的Key
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private static string GetServiceCallKey(AppCaller caller)
        {
            var cacheKey = string.Format("Caller_{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);
            return cacheKey.Replace(" ", "").Replace("\r\n", "").Replace("\t", "").ToLower();
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
                        Console.WriteLine("[{0}] => Queue length: {1} ({2}, {3}).", DateTime.Now, queue.Count, resMsg.ServiceName, resMsg.MethodName);

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

            if (enableCache)
            {
                var watch = Stopwatch.StartNew();

                //获取响应的消息
                var resMsg = ThreadManager.GetResponse(service, context, reqMsg);

                watch.Stop();

                wr.Set(resMsg);

                //设置响应信息到缓存
                SetResponseToCache(callKey, context, reqMsg, resMsg, watch.ElapsedMilliseconds);
            }
            else
            {
                //获取响应的消息
                var resMsg = ThreadManager.GetResponse(service, context, reqMsg);

                wr.Set(resMsg);
            }
        }

        /// <summary>
        /// 设置响应信息到缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedMilliseconds"></param>
        private void SetResponseToCache(string callKey, OperationContext context, RequestMessage reqMsg, ResponseMessage resMsg, long elapsedMilliseconds)
        {
            //如果符合条件，则自动缓存 【自动缓存功能】
            if (resMsg != null && !resMsg.IsError && resMsg.Count > 0)
            {
                var timeSpan = TimeSpan.FromSeconds(ServiceConfig.DEFAULT_RECORD_TIMEOUT);

                if (elapsedMilliseconds > timeSpan.TotalMilliseconds)
                {
                    CacheHelper.Permanent(callKey, resMsg);

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

        /// <summary>
        /// 从缓存中获取数据
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool GetResponseFromCache(string callKey, RequestMessage reqMsg, out ResponseMessage resMsg)
        {
            //从缓存中获取数据
            resMsg = CacheHelper.Get<ResponseMessage>(callKey);

            if (resMsg == null)
            {
                return false;
            }
            else
            {
                //刷新工作项
                ThreadManager.RefreshWorker(callKey);

                resMsg.ElapsedTime = 0;

                return true;
            }
        }
    }
}
