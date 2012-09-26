using System;
using System.Collections;
using System.Diagnostics;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Logger;

namespace MySoft.IoC.Services
{
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

        /// <summary>
        /// Lock object
        /// </summary>
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
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage Run(OperationContext context, RequestMessage reqMsg)
        {
            //如果是状态服务，则同步响应
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
            {
                return GetResponse(context, reqMsg);
            }

            //获取MethodCaller
            var caller = GetMethodCaller(context.Caller);

            lock (caller)
            {
                //获取CallerKey
                var callKey = GetCallerKey(reqMsg, context.Caller);

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

                //开始调用服务
                var ar = caller.BeginInvoke(context, reqMsg, null, null);

                //等待指定超时时间
                if (!ar.AsyncWaitHandle.WaitOne(waitTime))
                {
                    //获取超时响应信息
                    return GetTimeoutResponse(context, reqMsg);
                }
                else
                {
                    var resMsg = caller.EndInvoke(ar);
                    ar.AsyncWaitHandle.Close();

                    if (enabledCache)
                    {
                        //设置响应信息到缓存
                        SetResponseToCache(callKey, context, reqMsg, resMsg);
                    }

                    return resMsg;
                }
            }
        }

        /// <summary>
        /// 获取CallerKey
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(RequestMessage reqMsg, AppCaller caller)
        {
            //对Key进行组装
            return string.Format("{0}_Caller_{1}${2}${3}", (reqMsg.InvokeMethod ? "Invoke" : "Direct"),
                                caller.ServiceName, caller.MethodName, caller.Parameters)
                    .Replace(" ", "").Replace("\r\n", "").Replace("\t", "").ToLower();
        }

        /// <summary>
        /// 获取方法调用对象
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private AsyncMethodCaller GetMethodCaller(AppCaller caller)
        {
            //获取ServiceKey
            var serviceKey = string.Format("Service_{0}${1}", caller.ServiceName, caller.MethodName);

            //判断是否有锁Key存在
            if (!hashtable.ContainsKey(serviceKey))
            {
                hashtable[serviceKey] = new AsyncMethodCaller(GetResponse);
            }

            return hashtable[serviceKey] as AsyncMethodCaller;
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
                //获取异常响应信息
                resMsg = GetErrorResponse(context, reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(OperationContext context, RequestMessage reqMsg)
        {
            var body = string.Format("Remote client【{0}】async call service ({1},{2}) timeout ({4}) ms.\r\nParameters => {3}",
                reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString(), (int)waitTime.TotalMilliseconds);

            var error = IoCHelper.GetException(context, reqMsg, new TimeoutException(body));

            //写异常日志
            logger.WriteError(error);

            var title = string.Format("Async call service ({0},{1}) timeout ({2}) ms.",
                        reqMsg.ServiceName, reqMsg.MethodName, (int)waitTime.TotalMilliseconds);

            //处理异常
            return IoCHelper.GetResponse(reqMsg, new TimeoutException(title));
        }

        /// <summary>
        /// 获取异常响应信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private ResponseMessage GetErrorResponse(OperationContext context, RequestMessage reqMsg, Exception ex)
        {
            var body = string.Format("Remote client【{0}】async call service ({1},{2}) error.\r\nParameters => {3}",
                reqMsg.Message, reqMsg.ServiceName, reqMsg.MethodName, reqMsg.Parameters.ToString());

            var error = IoCHelper.GetException(context, reqMsg, body, ex);

            //写异常日志
            logger.WriteError(error);

            //处理异常
            return IoCHelper.GetResponse(reqMsg, ex);
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
            if (resMsg != null && resMsg.Value != null && !resMsg.IsError && resMsg.Count > 0)
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
                Value = resMsg.Value,
                ElapsedTime = 0
            };

            //如果是服务端，直接返回对象
            if (!fromServer && !reqMsg.InvokeMethod)
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    newMsg.Value = CoreHelper.CloneObject(newMsg.Value);

                    watch.Stop();

                    //设置耗时时间
                    newMsg.ElapsedTime = watch.ElapsedMilliseconds;
                }
                finally
                {
                    if (watch.IsRunning)
                    {
                        watch.Stop();
                    }
                }
            }

            return newMsg;
        }
    }
}