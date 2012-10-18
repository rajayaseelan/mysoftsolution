using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步调用器
    /// </summary>
    internal class AsyncCaller
    {
        /// <summary>
        /// Wait result table.
        /// </summary>
        private Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

        private IService service;
        private ICacheStrategy cache;
        private TimeSpan timeout;
        private bool enabledCache;
        private bool fromServer;
        private ThreadManager manager;
        private Random random;
        private AsyncMethodCaller caller;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, TimeSpan timeout, bool fromServer)
        {
            this.service = service;
            this.timeout = timeout;
            this.enabledCache = false;
            this.fromServer = fromServer;
            this.random = new Random();
            this.caller = new AsyncMethodCaller(GetInvokeResponse);
        }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(IService service, TimeSpan timeout, ICacheStrategy cache, bool fromServer)
            : this(service, timeout, fromServer)
        {
            this.cache = cache;
            this.enabledCache = true;
            this.manager = new ThreadManager(service, caller, cache);
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage Run(OperationContext context, RequestMessage reqMsg)
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

            //定义一个异步并发对象
            AsyncResult waitResult = null;

            lock (hashtable.SyncRoot)
            {
                //判断是否存在
                if (hashtable.ContainsKey(callKey))
                {
                    //获取异步并发对象
                    waitResult = hashtable[callKey] as AsyncResult;
                }
            }

            if (waitResult == null)
            {
                lock (hashtable.SyncRoot)
                {
                    //设置异步并发对象
                    waitResult = new AsyncResult();
                    hashtable[callKey] = waitResult;
                }

                try
                {
                    //开始调用服务
                    var resMsg = InvokeRequest(callKey, context, reqMsg);

                    //设置响应信息
                    waitResult.SetResponse(resMsg);

                    return resMsg;
                }
                finally
                {
                    lock (hashtable.SyncRoot)
                    {
                        hashtable.Remove(callKey);
                    }
                }
            }
            else
            {
                //返回响应结果
                return waitResult.GetResponse(reqMsg);
            }
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage InvokeRequest(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                //开始调用服务
                var ar = caller.BeginInvoke(context, reqMsg, null, null);

                //等待指定超时时间
                if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    //获取异常响应信息
                    return GetTimeoutResponse(reqMsg);
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
            catch (Exception ex)
            {
                //获取异常响应信息
                return IoCHelper.GetResponse(reqMsg, ex);
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
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg)
        {
            //获取异常响应信息
            var title = string.Format("Async call service ({0},{1}) timeout ({2}) ms.",
                        reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds);

            var resMsg = IoCHelper.GetResponse(reqMsg, new System.TimeoutException(title));

            //设置耗时时间
            resMsg.ElapsedTime = (long)timeout.TotalMilliseconds;

            return resMsg;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetInvokeResponse(OperationContext context, RequestMessage reqMsg)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            try
            {
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (ThreadAbortException ex)
            {
                //重置线程
                Thread.ResetAbort();

                //获取异常响应信息
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            catch (Exception ex)
            {
                //获取异常响应信息
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
            if (reqMsg.CacheTime <= 0) return;

            //如果符合条件，则自动缓存 【自动缓存功能】
            if (resMsg != null && resMsg.Value != null && !resMsg.IsError && resMsg.Count > 0)
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
                catch (Exception ex)
                {
                    //TODO
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