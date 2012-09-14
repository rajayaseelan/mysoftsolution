using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
    internal class AsyncCaller
    {
        private ILog logger;
        private IService service;
        private ICacheStrategy cache;
        private TimeSpan waitTime;
        private bool enabledCache;
        private ThreadManager manager;
        private Func<IService, OperationContext, RequestMessage, ResponseMessage> caller;
        private IDictionary<string, Queue<WaitResult>> results;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="waitTime"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan waitTime)
        {
            this.logger = logger;
            this.service = service;
            this.waitTime = waitTime;
            this.enabledCache = false;
            this.caller = GetResponse;
            this.results = new Dictionary<string, Queue<WaitResult>>();
        }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="waitTime"></param>
        /// <param name="cache"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan waitTime, ICacheStrategy cache)
            : this(logger, service, waitTime)
        {
            this.cache = cache;
            this.enabledCache = true;
            this.manager = new ThreadManager(cache, caller);
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
                return GetResponse(service, context, reqMsg);
            }

            //定义一个响应值
            ResponseMessage resMsg = null;

            var callKey = GetServiceCallKey(context.Caller);

            if (enabledCache)
            {
                //从缓存中获取数据
                if (GetResponseFromCache(callKey, reqMsg, out resMsg))
                {
                    //刷新数据
                    manager.RefreshWorker(callKey);

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

                        //开始调用服务
                        caller.BeginInvoke(service, context, reqMsg, AsyncCallback, new ArrayList { callKey, waitResult, context, reqMsg });
                    }
                    else
                    {
                        results[callKey].Enqueue(waitResult);
                    }
                }

                //等待指定超时时间
                if (!waitResult.WaitOne(waitTime))
                {
                    var body = string.Format("Remote client【{0}】async call service ({1},{2}) timeout ({4}) ms.\r\nParameters => {3}",
                        reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), (int)waitTime.TotalMilliseconds);

                    //获取异常
                    var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

                    logger.WriteError(error);

                    var title = string.Format("Async call service ({0},{1}) timeout ({2}) ms.",
                                reqMsg.ServiceName, reqMsg.MethodName, (int)waitTime.TotalMilliseconds);

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
                        IoCHelper.WriteLine(ConsoleColor.Cyan, "[{0}] => Queue length: {1} ({2}, {3}).", DateTime.Now, queue.Count, resMsg.ServiceName, resMsg.MethodName);

                        while (queue.Count > 0)
                        {
                            var wr = queue.Dequeue();

                            wr.SetResponse(resMsg);
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
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var arr = ar.AsyncState as ArrayList;

            var callKey = arr[0] as string;
            var wr = arr[1] as WaitResult;
            var context = arr[2] as OperationContext;
            var reqMsg = arr[3] as RequestMessage;

            var resMsg = caller.EndInvoke(ar);
            ar.AsyncWaitHandle.Close();

            if (enabledCache)
            {
                //设置响应信息到缓存
                SetResponseToCache(callKey, context, reqMsg, resMsg);
            }

            wr.SetResponse(resMsg);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            try
            {
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                //处理异常
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// 设置响应信息到缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        private void SetResponseToCache(string callKey, OperationContext context, RequestMessage reqMsg, ResponseMessage resMsg)
        {
            //如果符合条件，则自动缓存 【自动缓存功能】
            if (resMsg != null && !resMsg.IsError && resMsg.Count > 0)
            {
                //记录数大于100条
                if (reqMsg.CacheTime <= 0 && resMsg.Count > ServiceConfig.DEFAULT_CACHE_COUNT)
                {
                    //缓存5分钟
                    reqMsg.CacheTime = ServiceConfig.DEFAULT_CACHE_TIMEOUT;
                }

                if (reqMsg.CacheTime > 0)
                {
                    //克隆一个新的对象
                    var newMsg = NewResponseMessage(reqMsg, resMsg);

                    cache.InsertCache(callKey, newMsg, reqMsg.CacheTime * 10);

                    //Add worker item
                    var worker = new WorkerItem
                    {
                        CallKey = callKey,
                        Service = service,
                        Context = context,
                        Request = reqMsg,
                        SlidingTime = reqMsg.CacheTime,
                        UpdateTime = DateTime.Now.AddSeconds(reqMsg.CacheTime),
                        IsRunning = false
                    };

                    manager.AddWorker(callKey, worker);
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
            resMsg = cache.GetCache<ResponseMessage>(callKey);

            if (resMsg != null)
            {
                //克隆一个新的对象
                resMsg = NewResponseMessage(reqMsg, resMsg);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 产生一个新的对象
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private ResponseMessage NewResponseMessage(RequestMessage reqMsg, ResponseMessage resMsg)
        {
            var newMsg = new ResponseMessage
            {
                TransactionId = reqMsg.TransactionId,
                ReturnType = resMsg.ReturnType,
                ServiceName = resMsg.ServiceName,
                MethodName = resMsg.MethodName,
                Parameters = resMsg.Parameters,
                Error = resMsg.Error,
                Value = resMsg.Value
            };

            var watch = Stopwatch.StartNew();

            try
            {
                //如果不是Invoke方式调用，则返回克隆的对象
                if (!reqMsg.InvokeMethod)
                {
                    newMsg.Value = CoreHelper.CloneObject(resMsg.Value);
                }

                watch.Stop();
            }
            catch (Exception ex)
            {
                //TODO
            }
            finally
            {
                watch.Stop();
            }

            //计算耗时
            newMsg.ElapsedTime = watch.ElapsedMilliseconds;

            return newMsg;
        }
    }
}