using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
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
        private TimeSpan timeout;
        private bool enabledCache;
        private bool fromServer;

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan timeout, bool fromServer)
        {
            this.logger = logger;
            this.service = service;
            this.timeout = timeout;
            this.enabledCache = false;
            this.fromServer = fromServer;
        }

        /// <summary>
        /// 实例化AsyncCaller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="timeout"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public AsyncCaller(ILog logger, IService service, TimeSpan timeout, ICacheStrategy cache, bool fromServer)
            : this(logger, service, timeout, fromServer)
        {
            this.cache = cache;
            this.enabledCache = true;
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
            return string.Format("{0}_Caller_{1}_{2}${3}${4}", (reqMsg.InvokeMethod ? "Invoke" : "Direct")
                                , service.ServiceName, caller.ServiceName, caller.MethodName, caller.Parameters)
                                .Replace(" ", "").Replace("\r\n", "").Replace("\t", "").ToLower();
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage Run(OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            //获取CallerKey
            var callKey = GetCallerKey(reqMsg, context.Caller);

            if (enabledCache)
            {
                //从缓存中获取数据
                if (GetResponseFromCache(callKey, context, reqMsg, ref resMsg))
                {
                    return resMsg;
                }
            }

            //返回响应
            return InvokeResponse(context, reqMsg);
        }

        /// <summary>
        /// 同步或异步响应
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage InvokeResponse(OperationContext context, RequestMessage reqMsg)
        {
            if (reqMsg.ServiceName == typeof(IStatusService).FullName)
                return GetSyncResponse(context, reqMsg); //同步调用
            else
                return GetAsyncResponse(context, reqMsg); //异步调用
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetSyncResponse(OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            try
            {
                //设置上下文
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    //取消请求
                    Thread.ResetAbort();
                }

                //返回异常响应信息
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// 异常调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetAsyncResponse(OperationContext context, RequestMessage reqMsg)
        {
            //异步调用
            using (var worker = new WorkerItem(WaitCallback, context, reqMsg))
            {
                ResponseMessage resMsg = null;

                try
                {
                    //返回响应结果
                    resMsg = worker.GetResult(timeout);
                }
                catch (TimeoutException ex)
                {
                    //超时异常信息
                    resMsg = GetTimeoutResponse(reqMsg, ex);
                }
                catch (Exception ex)
                {
                    //处理异常响应
                    resMsg = IoCHelper.GetResponse(reqMsg, ex);
                }

                return resMsg;
            }
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            var worker = state as WorkerItem;

            try
            {
                //获取响应信息
                var resMsg = GetSyncResponse(worker.Context, worker.Request);

                worker.SetResult(resMsg);
            }
            catch (Exception ex)
            {
                //写异常日志
                logger.WriteError(ex);
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg, TimeoutException ex)
        {
            //获取异常响应信息
            var body = string.Format("Async call service ({0}, {1}) timeout ({2}) ms. {3}",
                        reqMsg.ServiceName, reqMsg.MethodName, (int)timeout.TotalMilliseconds, ex.Message);

            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(body));

            //设置耗时时间
            resMsg.ElapsedTime = (long)timeout.TotalMilliseconds;

            return resMsg;
        }

        /// <summary>
        /// 从缓存中获取数据
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool GetResponseFromCache(string callKey, OperationContext context, RequestMessage reqMsg, ref ResponseMessage resMsg)
        {
            //从缓存中获取数据
            if (reqMsg.CacheTime <= 0) return false;

            if (cache == null)
            {
                //双缓存保护获取方式
                var array = new ArrayList { context, reqMsg };

                resMsg = CacheHelper<ResponseMessage>.Get(callKey, TimeSpan.FromSeconds(reqMsg.CacheTime),
                        state =>
                        {
                            var arr = state as ArrayList;
                            var _context = arr[0] as OperationContext;
                            var _reqMsg = arr[1] as RequestMessage;

                            //异步请求响应数据
                            return InvokeResponse(_context, _reqMsg);

                        }, array, CheckResponse);
            }
            else
            {
                try
                {
                    //从缓存获取
                    resMsg = cache.GetObject<ResponseMessage>(callKey);
                }
                catch
                {
                }

                if (resMsg == null)
                {
                    //异步请求响应数据
                    resMsg = InvokeResponse(context, reqMsg);

                    if (CheckResponse(resMsg))
                    {
                        try
                        {
                            //插入缓存
                            cache.AddObject(callKey, resMsg, TimeSpan.FromSeconds(reqMsg.CacheTime));
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (resMsg != null)
            {
                //克隆一个新的对象
                resMsg = NewResponseMessage(reqMsg, resMsg);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 检测响应是否有效
        /// </summary>
        /// <param name="resMsg"></param>
        private bool CheckResponse(ResponseMessage resMsg)
        {
            if (resMsg == null) return false;

            //如果符合条件，则自动缓存 【自动缓存功能】
            if (!resMsg.IsError && resMsg.Value != null && resMsg.Count > 0)
            {
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
                ServiceName = resMsg.ServiceName,
                MethodName = resMsg.MethodName,
                Parameters = resMsg.Parameters,
                ElapsedTime = resMsg.ElapsedTime,
                Error = resMsg.Error,
                Value = resMsg.Value
            };

            //如果是服务端，直接返回对象
            if (!(fromServer || reqMsg.InvokeMethod))
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    newMsg.Value = CoreHelper.CloneObject(newMsg.Value);
                    watch.Stop();

                    //设置耗时
                    newMsg.ElapsedTime = watch.ElapsedMilliseconds;
                }
                catch (Exception ex) { }
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