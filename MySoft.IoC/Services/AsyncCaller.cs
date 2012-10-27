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
                var worker = GetWorkerItem(callKey, context, reqMsg, waitResult);

                //等待超时
                if (!waitResult.WaitOne(timeout))
                {
                    //结束线程
                    CancelThread(worker);

                    //获取超时响应
                    var resMsg = GetTimeoutResponse(reqMsg);

                    //设置响应信息
                    waitResult.SetResponse(resMsg);
                }

                //设置响应信息
                SetWorkerResponse(callKey, waitResult.Message);

                //返回响应结果
                return waitResult.Message;
            }
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        /// <param name="worker"></param>
        private void CancelThread(WorkerItem worker)
        {
            //结束线程
            if (worker.Thread != null)
            {
                try
                {
                    worker.Thread.Interrupt();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    worker.Thread = null;
                }
            }
        }

        /// <summary>
        /// 获取异常结果
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="waitResult"></param>
        /// <returns></returns>
        private WorkerItem GetWorkerItem(string callKey, OperationContext context, RequestMessage reqMsg, WaitResult waitResult)
        {
            var worker = new WorkerItem(waitResult)
            {
                CallKey = callKey,
                Context = context,
                Request = reqMsg
            };

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(callKey))
                {
                    //将waitResult加入队列中
                    hashtable[callKey] = new Queue<WaitResult>();

                    //启动线程调用
                    ThreadPool.QueueUserWorkItem(WaitCallback, worker);
                }
                else
                {
                    //加入队列中
                    var waitQueue = hashtable[callKey] as Queue<WaitResult>;
                    waitQueue.Enqueue(waitResult);
                }
            }

            return worker;
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
                var resMsg = GetWorkerResponse(worker);

                if (resMsg != null)
                {
                    if (enabledCache)
                    {
                        //设置响应信息到缓存
                        SetResponseToCache(worker, resMsg);
                    }

                    //设置响应信息
                    worker.Set(resMsg);
                }
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
                    var waitQueue = hashtable[callKey] as Queue<WaitResult>;

                    try
                    {
                        while (waitQueue.Count > 0)
                        {
                            try
                            {
                                //响应队列中的请求
                                var waitItem = waitQueue.Dequeue();

                                waitItem.SetResponse(resMsg);
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
        /// <param name="worker"></param>
        /// <returns></returns>
        private ResponseMessage GetWorkerResponse(WorkerItem worker)
        {
            //定义响应的消息
            ResponseMessage resMsg = null;

            //设置线程
            worker.Thread = Thread.CurrentThread;

            try
            {
                OperationContext.Current = worker.Context;

                //响应结果，清理资源
                resMsg = service.CallService(worker.Request);
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                //恢复线程
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                //获取异常响应信息
                resMsg = IoCHelper.GetResponse(worker.Request, ex);
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
        /// <param name="worker"></param>
        /// <param name="resMsg"></param>
        private void SetResponseToCache(WorkerItem worker, ResponseMessage resMsg)
        {
            var callKey = worker.CallKey;
            var reqMsg = worker.Request;
            var context = worker.Context;

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