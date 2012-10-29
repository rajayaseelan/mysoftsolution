using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Threading;

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
                    return resMsg;
                }
            }

            //异步调用
            return AsyncRun(callKey, context, reqMsg);
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage AsyncRun(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //异步响应
            using (var waitResult = new WaitResult(reqMsg))
            {
                //开始异步请求
                var worker = new WorkerItem(waitResult)
                {
                    CallKey = callKey,
                    Context = context,
                    Request = reqMsg,
                    IsAsyncRequest = false
                };

                //开始异步请求
                BeginAsyncRequest(worker);

                //等待超时
                if (!waitResult.WaitOne(timeout))
                {
                    if (worker.IsAsyncRequest)
                    {
                        RemoveKey(callKey);
                    }

                    //结束请求
                    worker.Cancel(timeout);
                    worker.Dispose();
                }
                else
                {
                    //设置响应信息
                    SetWorkerResponse(callKey, waitResult.Message);
                }

                //返回响应结果
                return waitResult.Message;
            }
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="callKey"></param>
        private void RemoveKey(string callKey)
        {
            lock (hashtable.SyncRoot)
            {
                if (hashtable.ContainsKey(callKey))
                {
                    hashtable.Remove(callKey);
                }
            }
        }

        /// <summary>
        /// 获取异常结果
        /// </summary>
        /// <param name="worker"></param>
        private void BeginAsyncRequest(WorkerItem worker)
        {
            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(worker.CallKey))
                {
                    worker.IsAsyncRequest = true;

                    //将waitResult加入队列中
                    hashtable[worker.CallKey] = new Queue<WorkerItem>();

                    //开始异步请求
                    ManagedThreadPool.QueueUserWorkItem(AsyncCallback, worker);
                }
                else
                {
                    //加入队列中
                    var workerQueue = hashtable[worker.CallKey] as Queue<WorkerItem>;
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
                worker.AsyncThread = Thread.CurrentThread;

                var callKey = worker.CallKey;
                var reqMsg = worker.Request;
                var context = worker.Context;

                //获取响应信息
                var resMsg = GetWorkerResponse(context, reqMsg);

                if (!worker.IsCompleted && resMsg != null)
                {
                    if (enabledCache)
                    {
                        //设置响应信息到缓存
                        SetResponseToCache(callKey, context, reqMsg, resMsg);
                    }

                    //设置响应信息
                    worker.Set(resMsg);
                    worker.Dispose();
                }
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
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
                    //获取队列
                    var workerQueue = hashtable[callKey] as Queue<WorkerItem>;

                    try
                    {
                        while (workerQueue.Count > 0)
                        {
                            try
                            {
                                //响应队列中的请求
                                var worker = workerQueue.Dequeue();

                                worker.Set(resMsg);
                                worker.Dispose();
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    finally
                    {
                        //移除队列
                        hashtable.Remove(callKey);
                    }
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
            catch (ThreadInterruptedException ex)
            {
                throw;
            }
            catch (ThreadAbortException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                return IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }
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

                //插入缓存
                cache.InsertCache(callKey, newMsg, reqMsg.CacheTime);
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
                var watch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    newMsg.Value = CoreHelper.CloneObject(newMsg.Value);

                    watch.Stop();

                    //设置耗时时间
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