using System;
using System.Collections;
using System.Collections.Generic;
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
        /// 用于存储并发队列信息
        /// </summary>
        private Hashtable hashtable = new Hashtable();

        private IService service;
        private ICacheStrategy cache;
        private TimeSpan timeout;
        private bool enabledCache;
        private bool fromServer;
        private Random random;

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

            //异步调用
            return AsyncRun(callKey, context, reqMsg);
        }

        /// <summary>
        /// 异常调用
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage AsyncRun(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //异步调用
            using (var waitResult = new WaitResult(reqMsg))
            using (var worker = new WorkerItem(waitResult) { Context = context, Request = reqMsg })
            {
                try
                {
                    //添加线程
                    ThreadManager.Set(context.Channel, worker);

                    //开始异步请求
                    QueueWorkerItem(callKey, worker);

                    //运行任务
                    return GetAsyncResponse(callKey, worker);
                }
                finally
                {
                    //移除结束
                    ThreadManager.Cancel(context.Channel);
                }
            }
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        private ResponseMessage GetAsyncResponse(string callKey, WorkerItem worker)
        {
            ResponseMessage resMsg = null;

            try
            {
                //返回响应结果
                resMsg = worker.GetResult(callKey, timeout, SetWorkerResponse);
            }
            catch (System.TimeoutException ex)
            {
                //超时响应
                resMsg = GetTimeoutResponse(worker.Request);
            }
            catch (Exception ex)
            {
                //处理异常响应
                resMsg = IoCHelper.GetResponse(worker.Request, ex);
            }

            return resMsg;
        }

        /// <summary>
        /// 获取异常结果
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="worker"></param>
        private void QueueWorkerItem(string callKey, WorkerItem worker)
        {
            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(callKey))
                {
                    //将waitResult加入队列中
                    hashtable[callKey] = new Queue<WorkerItem>();

                    //开始异步请求
                    ThreadPool.QueueUserWorkItem(AsyncCallback, worker);
                }
                else
                {
                    //加入队列中
                    var workerQueue = hashtable[callKey] as Queue<WorkerItem>;
                    workerQueue.Enqueue(worker);
                }
            }
        }

        /// <summary>
        /// 运行请求
        /// </summary>
        /// <param name="state"></param>
        private void AsyncCallback(object state)
        {
            var worker = state as WorkerItem;

            try
            {
                //设置线程
                worker.SetThread(Thread.CurrentThread);

                if (!worker.IsCompleted)
                {
                    //获取响应信息
                    var resMsg = GetWorkerResponse(worker.Context, worker.Request);

                    //设置响应信息
                    worker.SetResult(resMsg);
                }
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                //取消请求
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 设置响应信息
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="resMsg"></param>
        private void SetWorkerResponse(string callKey, ResponseMessage resMsg)
        {
            lock (hashtable.SyncRoot)
            {
                if (hashtable.ContainsKey(callKey))
                {
                    try
                    {
                        //获取队列
                        var queue = hashtable[callKey] as Queue<WorkerItem>;

                        while (queue.Count > 0)
                        {
                            try
                            {
                                //响应队列中的请求
                                var worker = queue.Dequeue();
                                worker.SetResult(resMsg);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        hashtable.Remove(callKey);
                    }
                }
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg)
        {
            //获取异常响应信息
            var title = string.Format("Async call service ({0}, {1}) timeout ({2}) ms.",
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
        private ResponseMessage GetWorkerResponse(OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                OperationContext.Current = context;

                //响应结果，清理资源
                return service.CallService(reqMsg);
            }
            finally
            {
                OperationContext.Current = null;
            }
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
                var array = new ArrayList { callKey, context, reqMsg };

                resMsg = CacheHelper<ResponseMessage>.Get(callKey, TimeSpan.FromSeconds(reqMsg.CacheTime),
                        state =>
                        {
                            var arr = state as ArrayList;
                            var _callKey = Convert.ToString(arr[0]);
                            var _context = arr[1] as OperationContext;
                            var _reqMsg = arr[2] as RequestMessage;

                            //异步请求响应数据
                            return AsyncRun(_callKey, _context, _reqMsg);

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
                    resMsg = AsyncRun(callKey, context, reqMsg);

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
            if (!fromServer && !reqMsg.InvokeMethod)
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