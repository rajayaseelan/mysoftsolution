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
        //用于缓存数据
        private Hashtable hashtable = new Hashtable();

        private IService service;
        private ICacheStrategy cache;
        private TimeSpan timeout;
        private bool enabledCache;
        private bool fromServer;

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
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                //取消请求
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
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

                //如果已经完成，直接返回
                if (worker.IsCompleted) return;

                worker.Set(resMsg);
            }
            catch
            {
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

                    if (CheckResponse(callKey, resMsg))
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
                resMsg = NewResponseMessage(callKey, reqMsg, resMsg);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 检测响应是否有效
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool CheckResponse(string callKey, ResponseMessage resMsg)
        {
            if (resMsg == null) return false;

            //如果符合条件，则缓存 
            if (!resMsg.IsError && resMsg.Count > 0)
            {
                try
                {
                    lock (hashtable.SyncRoot)
                    {
                        //将序列化数据存储在队列中
                        hashtable[callKey] = SerializationManager.SerializeBin(resMsg.Value);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 产生一个新的对象
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private ResponseMessage NewResponseMessage(string callKey, RequestMessage reqMsg, ResponseMessage resMsg)
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
            if (NeedCloneObject(reqMsg))
            {
                var watch = Stopwatch.StartNew();

                try
                {
                    //反序列化数据
                    newMsg.Value = CloneResponseMessage(callKey, newMsg.Value);

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

        /// <summary>
        /// 克隆ResponseMessage
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object CloneResponseMessage(string callKey, object value)
        {
            byte[] buffer = null;

            lock (hashtable.SyncRoot)
            {
                if (!hashtable.ContainsKey(callKey))
                {
                    //将序列化数据存储在队列中
                    hashtable[callKey] = SerializationManager.SerializeBin(value);
                }

                buffer = hashtable[callKey] as byte[];
            }

            //反序列化对象
            return SerializationManager.DeserializeBin(buffer);
        }

        /// <summary>
        /// 判断是否序列化
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedCloneObject(RequestMessage reqMsg)
        {
            return !(fromServer || reqMsg.InvokeMethod);
        }
    }
}