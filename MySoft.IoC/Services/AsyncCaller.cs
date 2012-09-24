using System;
using System.Collections;
using System.Diagnostics;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Logger;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="reqMsg"></param>
    /// <returns></returns>
    internal delegate ResponseMessage AsyncMethodCaller(OperationContext context, RequestMessage reqMsg);

    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        private ILog logger;
        private IService service;
        private ICacheStrategy cache;
        private TimeSpan waitTime;
        private bool enabledCache;
        private bool fromServer;
        private ThreadManager manager;
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="waitTime"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan waitTime, bool fromServer)
        {
            this.logger = logger;
            this.service = service;
            this.waitTime = waitTime;
            this.enabledCache = false;
            this.fromServer = fromServer;
        }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="waitTime"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan waitTime, ICacheStrategy cache, bool fromServer)
            : this(logger, service, waitTime, fromServer)
        {
            this.cache = cache;
            this.enabledCache = true;
            this.manager = new ThreadManager(service, GetResponse, cache);
        }

        /// <summary>
        /// 获取缓存的Key
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetServiceCallKey(AppCaller caller)
        {
            var cacheKey = string.Format("Caller_{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);
            return cacheKey.Replace(" ", "").Replace("\r\n", "").Replace("\t", "").ToLower();
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
                return GetResponse(context, reqMsg);
            }

            var callKey = GetServiceCallKey(context.Caller);

            if (enabledCache)
            {
                //定义一个响应值
                ResponseMessage resMsg = null;

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
                if (!hashtable.ContainsKey(callKey))
                {
                    //初始化队列
                    hashtable[callKey] = Queue.Synchronized(new Queue());

                    //工作项
                    var worker = new WorkerItem
                    {
                        CallKey = callKey,
                        Context = context,
                        Request = reqMsg
                    };

                    //开始调用服务
                    var caller = new AsyncMethodCaller(GetResponse);
                    caller.BeginInvoke(context, reqMsg, AsyncCallback, new ArrayList { waitResult, worker });
                }
                else
                {
                    //加入队列中
                    (hashtable[callKey] as Queue).Enqueue(waitResult);
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
                    var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(title));

                    waitResult.SetResponse(resMsg);
                }

                //返回响应消息
                return waitResult.Message;
            }
        }

        /// <summary>
        /// 设置响应
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="resMsg"></param>
        private void SetResponse(string callKey, ResponseMessage resMsg)
        {
            if (hashtable.ContainsKey(callKey))
            {
                var queue = hashtable[callKey] as Queue;

                if (queue.Count > 0)
                {
                    //输出队列信息
                    IoCHelper.WriteLine(ConsoleColor.Magenta, "[{0}] => Caller count: {1} ({2}, {3}).", DateTime.Now, queue.Count, resMsg.ServiceName, resMsg.MethodName);

                    while (queue.Count > 0)
                    {
                        var wr = queue.Dequeue() as WaitResult;

                        wr.SetResponse(resMsg);
                    }
                }
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="ar"></param>
        private void AsyncCallback(IAsyncResult ar)
        {
            var caller = (ar as AsyncResult).AsyncDelegate;
            var arr = ar.AsyncState as ArrayList;
            var wr = arr[0] as WaitResult;
            var worker = arr[1] as WorkerItem;

            try
            {
                var resMsg = (caller as AsyncMethodCaller).EndInvoke(ar);
                ar.AsyncWaitHandle.Close();

                //这里的resMsg居然会为null
                if (resMsg != null)
                {
                    if (enabledCache)
                    {
                        //设置响应信息到缓存
                        SetResponseToCache(worker.CallKey, worker.Context, worker.Request, resMsg);
                    }

                    //设置响应
                    wr.SetResponse(resMsg);

                    //设置队列响应信息
                    SetResponse(worker.CallKey, resMsg);
                }
            }
            finally
            {
                //移除指定的Key
                hashtable.Remove(worker.CallKey);
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
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
                    var newMsg = NewResponseMessage(reqMsg, resMsg, true);

                    cache.InsertCache(callKey, newMsg, reqMsg.CacheTime * 10);

                    //Add worker item
                    var worker = new WorkerItem
                    {
                        CallKey = callKey,
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
                resMsg = NewResponseMessage(reqMsg, resMsg, false);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 产生一个新的对象
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="isSerialize"></param>
        /// <returns></returns>
        private ResponseMessage NewResponseMessage(RequestMessage reqMsg, ResponseMessage resMsg, bool isSerialize)
        {
            //如果是服务端，直接返回对象
            if (fromServer)
            {
                return new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = resMsg.ReturnType,
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ElapsedTime = 0,
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };
            }
            else
            {
                //新值
                var newValue = resMsg.Value;

                var watch = Stopwatch.StartNew();

                try
                {
                    if (!reqMsg.InvokeMethod)
                    {
                        if (isSerialize)
                            newValue = SerializationManager.SerializeBin(newValue);
                        else
                            newValue = SerializationManager.DeserializeBin((byte[])newValue);
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
                return new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ReturnType = resMsg.ReturnType,
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ElapsedTime = watch.ElapsedMilliseconds,
                    Error = resMsg.Error,
                    Value = newValue
                };
            }
        }
    }
}